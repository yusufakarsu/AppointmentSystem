namespace AppointmentSystem.Data.Repositories
{
    using AppointmentSystem.Data.DbContext;
    using AppointmentSystem.Data.Entities;
    using AppointmentSystem.Data.Interfaces;
    using Microsoft.EntityFrameworkCore;

    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly AppDbContext _context;

        public AppointmentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<SalesManager>> GetSalesManagersAsync()
        {
            return await _context.SalesManagers
                .Include(sm => sm.Slots)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Slot>> GetAvailableSlotsAsync(DateTime requestedDate)
        {
            return await _context.Slots
                .Where(s => s.StartDate.Date == requestedDate.Date)
                .Include(s => s.SalesManager)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
