namespace LMS_API.Controllers;

using AutoMapper;
using LMS_API.Data;
using LMS_API.DTOs;
using LMS_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


    [ApiController]
    [Route("api/[controller]")]
    public class LeaveRequestsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public LeaveRequestsController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/leaverequests
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LeaveRequestDto>>> GetAll()
        {
            var leaveRequests = await _context.LeaveRequests
                .Include(l => l.Employee)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<LeaveRequestDto>>(leaveRequests));
        }

        // GET: api/leaverequests/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LeaveRequestDto>> Get(int id)
        {
            var leave = await _context.LeaveRequests
                .Include(l => l.Employee)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (leave == null) return NotFound();

            return Ok(_mapper.Map<LeaveRequestDto>(leave));
        }

    // POST: api/leaverequests
    [HttpPost]
    public async Task<ActionResult<LeaveRequestDto>> Create(CreateLeaveRequestDto dto)
    {
        var leaveRequest = _mapper.Map<LeaveRequest>(dto);
        leaveRequest.Status = LeaveStatus.Pending;
        leaveRequest.CreatedAt = DateTime.UtcNow;

        //Rule 1: No overlapping leave dates for the same employee
        bool overlaps = await _context.LeaveRequests.AnyAsync(lr =>
            lr.EmployeeId == leaveRequest.EmployeeId &&
            lr.StartDate <= leaveRequest.EndDate &&
            lr.EndDate >= leaveRequest.StartDate);

        if (overlaps)
            return BadRequest("The employee already has a leave request during this time period.");

        //Rule 2: Max 20 annual leave days per year
        if (leaveRequest.LeaveType == LeaveType.Annual)
        {
            var year = leaveRequest.StartDate.Year;
            var annualLeaves = await _context.LeaveRequests
                .Where(lr => lr.EmployeeId == leaveRequest.EmployeeId &&
                             lr.LeaveType == LeaveType.Annual &&
                             lr.StartDate.Year == year)
                .ToListAsync();

            int usedDays = annualLeaves.Sum(lr => (lr.EndDate - lr.StartDate).Days + 1);
            int requestedDays = (leaveRequest.EndDate - leaveRequest.StartDate).Days + 1;

            if (usedDays + requestedDays > 20)
                return BadRequest("Annual leave limit of 20 days exceeded for this year.");
        }

        //Rule 3: Sick leave must have a reason
        if (leaveRequest.LeaveType == LeaveType.Sick && string.IsNullOrWhiteSpace(leaveRequest.Reason))
            return BadRequest("Sick leave must include a reason.");

        //Save to DB
        _context.LeaveRequests.Add(leaveRequest);
        await _context.SaveChangesAsync();

        var created = await _context.LeaveRequests
            .Include(l => l.Employee)
            .FirstOrDefaultAsync(l => l.Id == leaveRequest.Id);

        return CreatedAtAction(nameof(Get), new { id = created.Id }, _mapper.Map<LeaveRequestDto>(created));
    }


    // PUT: api/leaverequests/5
    [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CreateLeaveRequestDto dto)
        {
            var leaveRequest = await _context.LeaveRequests.FindAsync(id);
            if (leaveRequest == null) return NotFound();

            _mapper.Map(dto, leaveRequest);
            _context.Entry(leaveRequest).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/leaverequests/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var leaveRequest = await _context.LeaveRequests.FindAsync(id);
            if (leaveRequest == null) return NotFound();

            _context.LeaveRequests.Remove(leaveRequest);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<LeaveRequest>>> FilterLeaveRequests(
        int? employeeId,
        LeaveType? leaveType,
        LeaveStatus? status,
        DateTime? startDate,
        DateTime? endDate,
        string? keyword,
        int page = 1,
        int pageSize = 10,
        string sortBy = "StartDate",
        string sortOrder = "asc")
        {
            var query = _context.LeaveRequests.AsQueryable();

            if (employeeId.HasValue)
                query = query.Where(lr => lr.EmployeeId == employeeId.Value);

            if (leaveType.HasValue)
                query = query.Where(lr => lr.LeaveType == leaveType.Value);

            if (status.HasValue)
                query = query.Where(lr => lr.Status == status.Value);

            if (startDate.HasValue)
                query = query.Where(lr => lr.StartDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(lr => lr.EndDate <= endDate.Value);

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(lr => lr.Reason.Contains(keyword));

            // Sorting
            switch (sortBy.ToLower())
            {
                case "startdate":
                    query = sortOrder == "desc" ? query.OrderByDescending(lr => lr.StartDate) : query.OrderBy(lr => lr.StartDate);
                    break;
                case "enddate":
                    query = sortOrder == "desc" ? query.OrderByDescending(lr => lr.EndDate) : query.OrderBy(lr => lr.EndDate);
                    break;
                default:
                    query = query.OrderBy(lr => lr.StartDate);
                    break;
            }

            // Pagination
            var totalRecords = await query.CountAsync();
            var leaveRequests = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                totalRecords,
                page,
                pageSize,
                data = leaveRequests
            });
        }

        [HttpGet("report")]
        public async Task<ActionResult<IEnumerable<object>>> GetReport([FromQuery] int year, [FromQuery] string? department, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var query = _context.LeaveRequests
                .Include(l => l.Employee)
                .Where(l => l.StartDate.Year == year);

            if (!string.IsNullOrEmpty(department))
                query = query.Where(l => l.Employee.Department == department);

            if (from.HasValue)
                query = query.Where(l => l.StartDate >= from.Value);

            if (to.HasValue)
                query = query.Where(l => l.EndDate <= to.Value);

            var result = await query
                .GroupBy(l => new { l.Employee.Id, l.Employee.FullName })
                .Select(g => new
                {
                    Employee = g.Key.FullName,
                    TotalLeaves = g.Count(),
                    AnnualLeaves = g.Count(l => l.LeaveType == LeaveType.Annual),
                    SickLeaves = g.Count(l => l.LeaveType == LeaveType.Sick)
                })
                .ToListAsync();

            return Ok(result);
        }


        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            var request = await _context.LeaveRequests.FindAsync(id);
            if (request == null) return NotFound();

            if (request.Status != LeaveStatus.Pending)
                return BadRequest("Only pending requests can be approved.");

            request.Status = LeaveStatus.Approved;
            await _context.SaveChangesAsync();

            return NoContent();
        }



}