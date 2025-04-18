﻿namespace LMS_API.DTOs;

    public class LeaveRequestDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string LeaveType { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; }
        public DateTime CreatedAt { get; set; }
    }