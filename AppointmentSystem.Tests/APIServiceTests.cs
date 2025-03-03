using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using AppointmentSystem.Api.Controllers;
using AppointmentSystem.Business.Services;
using AppointmentSystem.Models.DTO;

namespace AppointmentSystem.Tests.API
{
    [TestFixture]
    public class APIServiceTests
    {
        private Mock<IAppointmentService> _appointmentServiceMock;
        private Mock<ILogger<CalendarController>> _loggerMock;
        private CalendarController _controller;
        private List<string> _logMessages;

        [SetUp]
        public void SetUp()
        {
            _appointmentServiceMock = new Mock<IAppointmentService>();
            _loggerMock = new Mock<ILogger<CalendarController>>();
            _logMessages = new List<string>();

            // Capture log messages instead of direct LogInformation verification
            _loggerMock.Setup(l => l.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => CaptureLogMessage(v.ToString())),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ));

            _controller = new CalendarController(_appointmentServiceMock.Object, _loggerMock.Object);
        }

        private bool CaptureLogMessage(string message)
        {
            _logMessages.Add(message);
            return true;
        }

        [Test]
        public async Task GetAvailableSlots_NullRequest_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.GetAvailableSlots(null);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var badRequest = (BadRequestObjectResult)result;
            Assert.That(badRequest.StatusCode, Is.EqualTo(400));
            Assert.That(badRequest.Value?.GetType()?.GetProperty("message")?.GetValue(badRequest.Value), Is.EqualTo("Request body cannot be null."));

            // Logging should contain "Received appointment request"
            Assert.IsTrue(_logMessages.Any(msg => msg.Contains("Received appointment request")));

            // Ensure service is NOT called
            _appointmentServiceMock.Verify(s => s.GetAvailableSlotsAsync(It.IsAny<AppointmentRequestDto>()), Times.Never);
        }

        [Test]
        public async Task GetAvailableSlots_InvalidDateFormat_ReturnsBadRequest()
        {
            var request = new AppointmentRequestDto
            {
                Date = "invalid-date",
                Language = "English",
                Products = new List<string> { "SolarPanels" },
                Rating = "Gold"
            };

            _appointmentServiceMock
                .Setup(s => s.GetAvailableSlotsAsync(It.IsAny<AppointmentRequestDto>()))
                .ThrowsAsync(new ArgumentException("Invalid date format. Expected format: yyyy-MM-dd"));

            // Act
            var result = await _controller.GetAvailableSlots(request);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var badRequest = (BadRequestObjectResult)result;
            Assert.That(badRequest.StatusCode, Is.EqualTo(400));
            Assert.That(badRequest.Value?.GetType()?.GetProperty("message")?.GetValue(badRequest.Value), Is.EqualTo("Invalid date format. Expected format: yyyy-MM-dd"));

            _appointmentServiceMock.Verify(s => s.GetAvailableSlotsAsync(It.IsAny<AppointmentRequestDto>()), Times.Once);
        }

        [Test]
        public async Task GetAvailableSlots_MissingProducts_ReturnsBadRequest()
        {
            var request = new AppointmentRequestDto
            {
                Date = "2024-05-03",
                Language = "English",
                Products = new List<string>(),
                Rating = "Gold"
            };

            // Act
            var result = await _controller.GetAvailableSlots(request);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var badRequest = (BadRequestObjectResult)result;
            Assert.That(badRequest.StatusCode, Is.EqualTo(400));
            Assert.That(badRequest.Value?.GetType()?.GetProperty("message")?.GetValue(badRequest.Value), Is.EqualTo("At least one product must be selected."));

            // Ensure service is NOT called
            _appointmentServiceMock.Verify(s => s.GetAvailableSlotsAsync(It.IsAny<AppointmentRequestDto>()), Times.Never);
        }

        [Test]
        public async Task GetAvailableSlots_ValidRequest_ReturnsOk()
        {
            var request = new AppointmentRequestDto
            {
                Date = "2024-05-03",
                Language = "English",
                Products = new List<string> { "SolarPanels" },
                Rating = "Gold"
            };

            var mockSlots = new List<AvailableTimeSlotDto>
            {
                new() { StartDate = DateTime.Parse("2024-05-03T10:30:00Z"), AvailableCount = 2 },
                new() { StartDate = DateTime.Parse("2024-05-03T11:30:00Z"), AvailableCount = 1 }
            };

            _appointmentServiceMock
                .Setup(s => s.GetAvailableSlotsAsync(It.IsAny<AppointmentRequestDto>()))
                .ReturnsAsync(mockSlots);

            // Act
            var result = await _controller.GetAvailableSlots(request);

            // Assert
            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var okResult = (OkObjectResult)result;
            Assert.That(okResult.StatusCode, Is.EqualTo(200));
            Assert.NotNull(okResult.Value);

            var returnedSlots = okResult.Value as List<AvailableTimeSlotDto>;
            Assert.That(returnedSlots?.Count, Is.EqualTo(2));
            Assert.That(returnedSlots[0].StartDate, Is.EqualTo(mockSlots[0].StartDate));
            Assert.That(returnedSlots[1].StartDate, Is.EqualTo(mockSlots[1].StartDate));

            _appointmentServiceMock.Verify(s => s.GetAvailableSlotsAsync(It.IsAny<AppointmentRequestDto>()), Times.Once);
        }

        [Test]
        public async Task GetAvailableSlots_ServiceThrowsException_ReturnsInternalServerError()
        {
            var request = new AppointmentRequestDto
            {
                Date = "2024-05-03",
                Language = "English",
                Products = new List<string> { "SolarPanels" },
                Rating = "Gold"
            };

            _appointmentServiceMock
                .Setup(s => s.GetAvailableSlotsAsync(It.IsAny<AppointmentRequestDto>()))
                .ThrowsAsync(new Exception("Something went wrong"));

            // Act
            var result = await _controller.GetAvailableSlots(request);

            // Assert
            Assert.That(result, Is.TypeOf<ObjectResult>());
            var errorResult = (ObjectResult)result;
            Assert.That(errorResult.StatusCode, Is.EqualTo(500));
            Assert.That(errorResult.Value?.GetType()?.GetProperty("message")?.GetValue(errorResult.Value), Is.EqualTo("An unexpected error occurred. Please try again later."));

            _appointmentServiceMock.Verify(s => s.GetAvailableSlotsAsync(It.IsAny<AppointmentRequestDto>()), Times.Once);
        }
    }
}
