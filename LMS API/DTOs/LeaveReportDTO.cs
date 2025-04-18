namespace LMS_API.DTOs;

public class LeaveReportDto
{
    public int EmployeeId { get; set; }
    public string FullName { get; set; }
    public int TotalLeaves { get; set; }
    public int AnnualLeaves { get; set; }
    public int SickLeaves { get; set; }
}


