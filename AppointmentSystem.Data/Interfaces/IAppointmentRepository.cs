namespace AppointmentSystem.Data.Interfaces
{
    using AppointmentSystem.Data.Entities;

    public interface IAppointmentRepository
    {
        Task<List<SalesManager>> GetSalesManagersAsync();
        Task<List<Slot>> GetAvailableSlotsAsync(DateTime requestedDate);
    }
}
