using LMS_API.Models;

namespace LMS_API.DTOs;

public class CreateLeaveRequestDto
{
    public int EmployeeId { get; set; }
    public LeaveType LeaveType { get; set; }   // ← now an enum
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; }
}




