using AppointmentSystem.Business.Extensions;
using AppointmentSystem.Data.Entities;
using AppointmentSystem.Data.Interfaces;
using AppointmentSystem.Models.DTO;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace AppointmentSystem.Business.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _repository;
        private readonly ILogger<AppointmentService> _logger;

        public AppointmentService(IAppointmentRepository repository, ILogger<AppointmentService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<List<AvailableTimeSlotDto>> GetAvailableSlotsAsync(AppointmentRequestDto request)
        {
            _logger.LogInformation("📌 Received appointment request: {@Request}", request);

            if (!TryParseDate(request.Date, out var requestedDate))
            {
                throw new ArgumentException("Invalid date format. Expected format: yyyy-MM-dd");
            }

            var salesManagers = await _repository.GetSalesManagersAsync();
            var matchingManagers = GetEligibleSalesManagers(salesManagers, request);

            if (!matchingManagers.Any())
            {
                _logger.LogInformation("No matching sales managers found.");
                return new List<AvailableTimeSlotDto>();
            }

            var availableSlots = GetAvailableSlots(matchingManagers, requestedDate);
            return FormatAvailableSlots(availableSlots);
        }

        private static bool TryParseDate(string date, out DateTime parsedDate)
        {
            return DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate);
        }

        private List<SalesManager> GetEligibleSalesManagers(List<SalesManager> salesManagers, AppointmentRequestDto request)
        {
            var matchingManagers = salesManagers
                .Where(manager =>
                    manager.LanguagesHashSet.Contains(request.Language) &&
                    request.Products.All(product => manager.ProductsHashSet.Contains(product)) &&
                    manager.CustomerRatingsHashSet.Contains(request.Rating))
                .ToList();

            _logger.LogInformation("Found {Count} matching Sales Managers: {@Managers}", matchingManagers.Count, matchingManagers);
            return matchingManagers;
        }

        private List<Slot> GetAvailableSlots(List<SalesManager> matchingManagers, DateTime requestedDate)
        {
            var allSlots = matchingManagers
                .SelectMany(manager => manager.Slots)
                .Where(slot => slot.StartDate.Date == requestedDate.Date)
                .ToList();

            var bookedSlots = allSlots.Where(slot => slot.Booked).ToList();
            var availableSlots = allSlots.Where(slot => !slot.Booked).ToList();

            return availableSlots.RemoveOverlappingSlots(bookedSlots);
        }

        private List<AvailableTimeSlotDto> FormatAvailableSlots(List<Slot> slots)
        {
            var formattedSlots = slots
                .GroupBy(slot => slot.StartDate)
                .Select(group => new AvailableTimeSlotDto
                {
                    StartDate = group.Key,
                    AvailableCount = group.Select(slot => slot.SalesManagerId).Distinct().Count()
                })
                .OrderBy(slot => slot.StartDate)
                .ToList();

            _logger.LogInformation("✅ Returning {Count} formatted slots: {@FormattedSlots}", formattedSlots.Count, formattedSlots);
            return formattedSlots;
        }
    }
}
