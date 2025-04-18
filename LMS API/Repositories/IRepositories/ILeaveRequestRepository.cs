namespace LMS_API.Repositories.IRepositories;

using LMS_API.Models;
using LMS_API.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ILeaveRequestRepository
{
    Task<IEnumerable<LeaveRequest>> GetAllAsync();
    Task<LeaveRequest?> GetByIdAsync(int id);
    Task<LeaveRequest> CreateAsync(LeaveRequest leaveRequest);
    Task UpdateAsync(LeaveRequest leaveRequest);
    Task DeleteAsync(int id);

    Task<(IEnumerable<LeaveRequest> Data, int TotalRecords)>
        FilterAsync(int? employeeId,
                    LeaveType? leaveType,
                    LeaveStatus? status,
                    DateTime? startDate,
                    DateTime? endDate,
                    string? keyword,
                    int page,
                    int pageSize,
                    string sortBy,
                    string sortOrder);

    Task<IEnumerable<LeaveReportDto>>
        GetReportAsync(int year,
                       string? department,
                       DateTime? from,
                       DateTime? to);

    Task ApproveAsync(int id);
}