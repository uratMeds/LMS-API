namespace LMS_API.Repositories.IRepositories;

using LMS_API.Data;
using LMS_API.DTOs;
using LMS_API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class LeaveRequestRepository : ILeaveRequestRepository
{
    private readonly AppDbContext _context;
    public LeaveRequestRepository(AppDbContext context)
        => _context = context;

    public async Task<IEnumerable<LeaveRequest>> GetAllAsync()
        => await _context.LeaveRequests
                          .Include(l => l.Employee)
                          .ToListAsync();

    public async Task<LeaveRequest?> GetByIdAsync(int id)
        => await _context.LeaveRequests
                          .Include(l => l.Employee)
                          .FirstOrDefaultAsync(l => l.Id == id);

    public async Task<LeaveRequest> CreateAsync(LeaveRequest leaveRequest)
    {
        _context.LeaveRequests.Add(leaveRequest);
        await _context.SaveChangesAsync();
        return leaveRequest;
    }

    public async Task UpdateAsync(LeaveRequest leaveRequest)
    {
        _context.Entry(leaveRequest).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.LeaveRequests.FindAsync(id);
        if (entity == null) return;
        _context.LeaveRequests.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<(IEnumerable<LeaveRequest> Data, int TotalRecords)>
        FilterAsync(int? employeeId,
                    LeaveType? leaveType,
                    LeaveStatus? status,
                    DateTime? startDate,
                    DateTime? endDate,
                    string? keyword,
                    int page,
                    int pageSize,
                    string sortBy,
                    string sortOrder)
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
        query = sortBy.ToLower() switch
        {
            "enddate" => sortOrder == "desc"
                ? query.OrderByDescending(lr => lr.EndDate)
                : query.OrderBy(lr => lr.EndDate),
            _ => sortOrder == "desc"
                ? query.OrderByDescending(lr => lr.StartDate)
                : query.OrderBy(lr => lr.StartDate),
        };

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<IEnumerable<LeaveReportDto>>
        GetReportAsync(int year,
                       string? department,
                       DateTime? from,
                       DateTime? to)
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

        return await query
            .GroupBy(l => new { l.Employee.Id, l.Employee.FullName })
            .Select(g => new LeaveReportDto
            {
                EmployeeId = g.Key.Id,
                FullName = g.Key.FullName,
                TotalLeaves = g.Count(),
                AnnualLeaves = g.Count(l => l.LeaveType == LeaveType.Annual),
                SickLeaves = g.Count(l => l.LeaveType == LeaveType.Sick),
            })
            .ToListAsync();

    }

    public async Task ApproveAsync(int id)
    {
        var entity = await _context.LeaveRequests.FindAsync(id);
        if (entity == null) throw new KeyNotFoundException();
        entity.Status = LeaveStatus.Approved;
        await _context.SaveChangesAsync();
    }
}