namespace AppointmentSystem.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;
    using AppointmentSystem.Business.Services;
    using AppointmentSystem.Models.DTO;

    [ApiController]
    [Route("calendar")]
    public class CalendarController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly ILogger<CalendarController> _logger;

        public CalendarController(IAppointmentService appointmentService, ILogger<CalendarController> logger)
        {
            _appointmentService = appointmentService;
            _logger = logger;

        }

        [HttpPost("query")]
        public async Task<IActionResult> GetAvailableSlots([FromBody] AppointmentRequestDto request)
        {
            _logger.LogInformation("Received appointment request: {@Request}", request);

            if (request == null)
            {
                return BadRequest(new { message = "Request body cannot be null." });
            }

            if (string.IsNullOrWhiteSpace(request.Date))
            {
                return BadRequest(new { message = "Date is required." });
            }

            if (request.Products == null || !request.Products.Any())
            {
                return BadRequest(new { message = "At least one product must be selected." });
            }

            if (string.IsNullOrWhiteSpace(request.Language))
            {
                return BadRequest(new { message = "Language is required." });
            }

            if (string.IsNullOrWhiteSpace(request.Rating))
            {
                return BadRequest(new { message = "Customer rating is required." });
            }

            try
            {
                var availableSlots = await _appointmentService.GetAvailableSlotsAsync(request);

                _logger.LogInformation("Filtered available slots: {@AvailableSlots}", availableSlots);

                return Ok(availableSlots);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Validation error: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while processing the request.");
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
            }
        }

    }
}
