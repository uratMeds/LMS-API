namespace LMS_API.Data;

using Microsoft.EntityFrameworkCore;
using LMS_API.Models;


    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed some data
            modelBuilder.Entity<Employee>().HasData(
            new Employee
                {
                    Id = 1,
                    FullName = "John Doe",
                    Department = "IT",
                    JoiningDate = new DateTime(2022, 1, 15)
                }
            );


            modelBuilder.Entity<LeaveRequest>().HasData(
                new LeaveRequest
                {
                    Id = 1,
                    EmployeeId = 1,
                    LeaveType = LeaveType.Annual,
                    StartDate = new DateTime(2024, 4, 18),
                    EndDate = new DateTime(2024, 4, 21),
                    Status = LeaveStatus.Pending,
                    Reason = "Vacation",
                    CreatedAt = new DateTime(2024, 4, 16)
                }
            );
        }
    }