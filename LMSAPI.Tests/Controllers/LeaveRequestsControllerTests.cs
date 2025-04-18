namespace LMSAPI.Tests.Controllers;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using LMS_API.Controllers;
using LMS_API.DTOs;
using LMS_API.Models;
using LMS_API.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;


    public class LeaveRequestsControllerTests
    {
        private readonly Mock<ILeaveRequestRepository> _repoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly LeaveRequestsController _controller;

        public LeaveRequestsControllerTests()
        {
            _repoMock = new Mock<ILeaveRequestRepository>();
            _mapperMock = new Mock<IMapper>();
            _controller = new LeaveRequestsController(_repoMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOk_WithDtoList()
        {
            // Arrange
            var entities = new List<LeaveRequest> {
                new LeaveRequest { Id = 1, Reason = "A" },
                new LeaveRequest { Id = 2, Reason = "B" }
            };
            var dtos = new List<LeaveRequestDto> {
                new LeaveRequestDto { Id = 1, Reason = "A" },
                new LeaveRequestDto { Id = 2, Reason = "B" }
            };

            _repoMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(entities);

            _mapperMock
                .Setup(m => m.Map<IEnumerable<LeaveRequestDto>>(entities))
                .Returns(dtos);

            // Act
            var actionResult = await _controller.GetAll();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Same(dtos, ok.Value);
        }

        [Fact]
        public async Task Get_ById_NotFound_WhenNull()
        {
            // Arrange
            _repoMock
                .Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync((LeaveRequest?)null);

            // Act
            var actionResult = await _controller.Get(5);

            // Assert
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }

        [Fact]
        public async Task Get_ById_ReturnsOk_WithDto()
        {
            // Arrange
            var entity = new LeaveRequest { Id = 7, Reason = "Test" };
            var dto = new LeaveRequestDto { Id = 7, Reason = "Test" };

            _repoMock
                .Setup(r => r.GetByIdAsync(7))
                .ReturnsAsync(entity);

            _mapperMock
                .Setup(m => m.Map<LeaveRequestDto>(entity))
                .Returns(dto);

            // Act
            var actionResult = await _controller.Get(7);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Same(dto, ok.Value);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtAction_WithDto()
        {
            // Arrange
            var createDto = new CreateLeaveRequestDto
            {
                EmployeeId = 3,
                LeaveType = LeaveType.Annual,
                StartDate = new DateTime(2025, 1, 1),
                EndDate = new DateTime(2025, 1, 2),
                Reason = "Holiday"
            };
            var entity = new LeaveRequest
            {
                Id = 42,
                EmployeeId = 3,
                LeaveType = LeaveType.Annual,
                StartDate = createDto.StartDate,
                EndDate = createDto.EndDate,
                Reason = createDto.Reason,
                Status = LeaveStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            var returnedDto = new LeaveRequestDto
            {
                Id = 42,
                EmployeeId = 3,
                LeaveType = LeaveType.Annual.ToString(),
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                Reason = entity.Reason,
                Status = LeaveStatus.Pending.ToString(),
                CreatedAt = entity.CreatedAt
            };

            _mapperMock
                .Setup(m => m.Map<LeaveRequest>(createDto))
                .Returns(entity);

            _repoMock
                .Setup(r => r.CreateAsync(entity))
                .ReturnsAsync(entity);

            _repoMock
                .Setup(r => r.GetByIdAsync(entity.Id))
                .ReturnsAsync(entity);

            _mapperMock
                .Setup(m => m.Map<LeaveRequestDto>(entity))
                .Returns(returnedDto);

            // Act
            var actionResult = await _controller.Create(createDto);

            // Assert
            var createdAt = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            Assert.Equal(nameof(_controller.Get), createdAt.ActionName);
            Assert.Equal(42, ((LeaveRequestDto)createdAt.Value!).Id);
        }

        [Fact]
        public async Task Approve_ReturnsNoContent()
        {
            // Arrange
            _repoMock
                .Setup(r => r.ApproveAsync(99))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Approve(99);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }