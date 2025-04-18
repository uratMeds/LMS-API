namespace LMS_API.Models;

public enum LeaveType
{
    Annual,
    Sick,
    Other
}

public enum LeaveStatus
{
    Pending,
    Approved,
    Rejected
}

public class LeaveRequest
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; }
    public LeaveType LeaveType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public LeaveStatus Status { get; set; }
    public string Reason { get; set; }
    public DateTime CreatedAt { get; set; }
}

