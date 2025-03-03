namespace AppointmentSystem.Business.Services
{
    using AppointmentSystem.Models.DTO;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IAppointmentService
    {
        Task<List<AvailableTimeSlotDto>> GetAvailableSlotsAsync(AppointmentRequestDto request);
    }
}
