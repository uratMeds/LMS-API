namespace LMSAPI.Tests.Models;

using System;
using System.Collections.Generic;
using Xunit;
using LMS_API.Models;

public class EmployeeTests
    {
        [Fact]
        public void Employee_Properties_Should_Be_Set_And_Retrieved_Correctly()
        {
            // Arrange
            var employee = new Employee
            {
                Id = 1,
                FullName = "John Doe",
                Department = "HR",
                JoiningDate = new DateTime(2020, 1, 1),
                LeaveRequests = new List<LeaveRequest>()
            };

            // Act & Assert
            Assert.Equal(1, employee.Id);
            Assert.Equal("John Doe", employee.FullName);
            Assert.Equal("HR", employee.Department);
            Assert.Equal(new DateTime(2020, 1, 1), employee.JoiningDate);
            Assert.NotNull(employee.LeaveRequests);
            Assert.Empty(employee.LeaveRequests);
        }

        [Fact]
        public void Employee_LeaveRequests_Should_Be_Manageable()
        {
            // Arrange
            var employee = new Employee
            {
                LeaveRequests = new List<LeaveRequest>()
            };

            var leaveRequest = new LeaveRequest
            {
                Id = 1,
                StartDate = new DateTime(2023, 5, 1),
                EndDate = new DateTime(2023, 5, 5),
                Reason = "Vacation"
            };

            // Act
            employee.LeaveRequests.Add(leaveRequest);

            // Assert
            Assert.Single(employee.LeaveRequests);
            Assert.Contains(leaveRequest, employee.LeaveRequests);
        }
    }