﻿namespace LMS_API.DTOs;

    public class CreateLeaveRequestDto
    {
        public int EmployeeId { get; set; }
        public string LeaveType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; }
    }