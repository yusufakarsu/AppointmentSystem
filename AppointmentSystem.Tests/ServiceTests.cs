namespace AppointmentSystem.Tests.Services
{
    using AppointmentSystem.Business.Services;
    using AppointmentSystem.Data.Entities;
    using AppointmentSystem.Data.Interfaces;
    using AppointmentSystem.Models.DTO;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [TestFixture]
    public class ServiceTests
    {
        private Mock<IAppointmentRepository> _repositoryMock;
        private Mock<ILogger<AppointmentService>> _loggerMock;
        private AppointmentService _appointmentService;

        [SetUp]
        public void Setup()
        {
            _repositoryMock = new Mock<IAppointmentRepository>();
            _loggerMock = new Mock<ILogger<AppointmentService>>();
            _appointmentService = new AppointmentService(_repositoryMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task GetAvailableSlotsAsync_InvalidDateFormat_ThrowsException()
        {
            // Arrange
            var request = new AppointmentRequestDto { Date = "invalid-date" };

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _appointmentService.GetAvailableSlotsAsync(request));

            Assert.That(ex.Message, Is.EqualTo("Invalid date format. Expected format: yyyy-MM-dd"));
        }


        [Test]
        public async Task GetAvailableSlotsAsync_NoMatchingSalesManagers_ReturnsEmptyList()
        {
            // Arrange
            var request = new AppointmentRequestDto { Date = "2024-03-10", Language = "English", Products = new List<string> { "ProductA" }, Rating = "5" };
            _repositoryMock.Setup(repo => repo.GetSalesManagersAsync()).ReturnsAsync(new List<SalesManager>());

            // Act
            var result = await _appointmentService.GetAvailableSlotsAsync(request);

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetAvailableSlotsAsync_MatchingManagersButNoSlots_ReturnsEmptyList()
        {
            // Arrange
            var request = new AppointmentRequestDto { Date = "2024-03-10", Language = "English", Products = new List<string> { "ProductA" }, Rating = "5" };
            var salesManagers = new List<SalesManager>
            {
                new SalesManager { Id = 1, Languages = new List<string> { "English" }, Products = new List<string> { "ProductA" }, CustomerRatings = new List<string> { "5" }, Slots = new List<Slot>() }
            };

            _repositoryMock.Setup(repo => repo.GetSalesManagersAsync()).ReturnsAsync(salesManagers);

            // Act
            var result = await _appointmentService.GetAvailableSlotsAsync(request);

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetAvailableSlotsAsync_ReturnsCorrectlyFilteredSlots()
        {
            // Arrange
            var request = new AppointmentRequestDto { Date = "2024-03-10", Language = "English", Products = new List<string> { "ProductA" }, Rating = "5" };

            var salesManagers = new List<SalesManager>
            {
                new SalesManager
                {
                    Id = 1,
                    Languages = new List<string> { "English" },
                    Products = new List<string> { "ProductA" },
                    CustomerRatings = new List<string> { "5" },
                    Slots = new List<Slot>
                    {
                        new Slot { Id = 1, SalesManagerId = 1, StartDate = new DateTime(2024, 3, 10, 10, 0, 0), EndDate = new DateTime(2024, 3, 10, 11, 0, 0), Booked = false },
                        new Slot { Id = 2, SalesManagerId = 1, StartDate = new DateTime(2024, 3, 10, 11, 0, 0), EndDate = new DateTime(2024, 3, 10, 12, 0, 0), Booked = false }
                    }
                }
            };

            _repositoryMock.Setup(repo => repo.GetSalesManagersAsync()).ReturnsAsync(salesManagers);

            // Act
            var result = await _appointmentService.GetAvailableSlotsAsync(request);

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].StartDate, Is.EqualTo(new DateTime(2024, 3, 10, 10, 0, 0)));
            Assert.That(result[0].AvailableCount, Is.EqualTo(1));
            Assert.That(result[1].StartDate, Is.EqualTo(new DateTime(2024, 3, 10, 11, 0, 0)));
        }

        [Test]
        public async Task GetAvailableSlotsAsync_HandlesOverlappingSlotsCorrectly()
        {
            // Arrange
            var request = new AppointmentRequestDto { Date = "2024-03-10", Language = "English", Products = new List<string> { "ProductA" }, Rating = "5" };

            var salesManagers = new List<SalesManager>
            {
                new SalesManager
                {
                    Id = 1,
                    Languages = new List<string> { "English" },
                    Products = new List<string> { "ProductA" },
                    CustomerRatings = new List<string> { "5" },
                    Slots = new List<Slot>
                    {
                        new Slot { Id = 1, SalesManagerId = 1, StartDate = new DateTime(2024, 3, 10, 10, 0, 0), EndDate = new DateTime(2024, 3, 10, 11, 0, 0), Booked = true },
                        new Slot { Id = 2, SalesManagerId = 1, StartDate = new DateTime(2024, 3, 10, 10, 30, 0), EndDate = new DateTime(2024, 3, 10, 11, 30, 0), Booked = false }
                    }
                }
            };

            _repositoryMock.Setup(repo => repo.GetSalesManagersAsync()).ReturnsAsync(salesManagers);

            // Act
            var result = await _appointmentService.GetAvailableSlotsAsync(request);

            // Assert
            Assert.That(result, Is.Empty, "Overlapping slot should be removed");
        }

        [Test]
        public async Task GetAvailableSlotsAsync_GroupsByStartDateAndCountsUniqueManagers()
        {
            // Arrange
            var request = new AppointmentRequestDto { Date = "2024-03-10", Language = "English", Products = new List<string> { "ProductA" }, Rating = "5" };

            var salesManagers = new List<SalesManager>
            {
                new SalesManager
                {
                    Id = 1,
                    Languages = new List<string> { "English" },
                    Products = new List<string> { "ProductA" },
                    CustomerRatings = new List<string> { "5" },
                    Slots = new List<Slot>
                    {
                        new Slot { Id = 1, SalesManagerId = 1, StartDate = new DateTime(2024, 3, 10, 10, 0, 0), EndDate = new DateTime(2024, 3, 10, 11, 0, 0), Booked = false },
                        new Slot { Id = 2, SalesManagerId = 2, StartDate = new DateTime(2024, 3, 10, 10, 0, 0), EndDate = new DateTime(2024, 3, 10, 11, 0, 0), Booked = false }
                    }
                }
            };

            _repositoryMock.Setup(repo => repo.GetSalesManagersAsync()).ReturnsAsync(salesManagers);

            // Act
            var result = await _appointmentService.GetAvailableSlotsAsync(request);

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].StartDate, Is.EqualTo(new DateTime(2024, 3, 10, 10, 0, 0)));
            Assert.That(result[0].AvailableCount, Is.EqualTo(2), "Two different managers should count as 2 available slots");
        }
    }
}
