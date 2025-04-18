namespace LMS_API.Controllers;

using AutoMapper;
using LMS_API.DTOs;
using LMS_API.Models;
using LMS_API.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class LeaveRequestsController : ControllerBase
{
    private readonly ILeaveRequestRepository _repo;
    private readonly IMapper _mapper;

    public LeaveRequestsController(ILeaveRequestRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LeaveRequestDto>>> GetAll()
    {
        var list = await _repo.GetAllAsync();
        return Ok(_mapper.Map<IEnumerable<LeaveRequestDto>>(list));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LeaveRequestDto>> Get(int id)
    {
        var leave = await _repo.GetByIdAsync(id);
        if (leave == null) return NotFound();
        return Ok(_mapper.Map<LeaveRequestDto>(leave));
    }

    [HttpPost]
    public async Task<ActionResult<LeaveRequestDto>> Create(CreateLeaveRequestDto dto)
    {
        var entity = _mapper.Map<LeaveRequest>(dto);
        entity.Status = LeaveStatus.Pending;
        entity.CreatedAt = DateTime.UtcNow;

        // ... keep your business-rule checks here ...

        await _repo.CreateAsync(entity);
        var created = await _repo.GetByIdAsync(entity.Id);
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, _mapper.Map<LeaveRequestDto>(created));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CreateLeaveRequestDto dto)
    {
        var leave = await _repo.GetByIdAsync(id);
        if (leave == null) return NotFound();
        _mapper.Map(dto, leave);
        await _repo.UpdateAsync(leave);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var leave = await _repo.GetByIdAsync(id);
        if (leave == null) return NotFound();
        await _repo.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("filter")]
    public async Task<ActionResult> Filter(
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
        var (entities, total) = await _repo.FilterAsync(
            employeeId, leaveType, status, startDate, endDate, keyword,
            page, pageSize, sortBy, sortOrder);

        var dtos = _mapper.Map<IEnumerable<LeaveRequestDto>>(entities);

        return Ok(new
        {
            total,
            page,
            pageSize,
            data = dtos
        });

    }

    [HttpGet("report")]
    public async Task<ActionResult<IEnumerable<LeaveReportDto>>> Report(
        [FromQuery] int year,
        [FromQuery] string? department,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var result = await _repo.GetReportAsync(year, department, from, to);
        return Ok(result);
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        await _repo.ApproveAsync(id);
        return NoContent();
    }
}
