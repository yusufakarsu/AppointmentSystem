namespace AppointmentSystem.Data.DbContext
{
    using Microsoft.EntityFrameworkCore;
    using AppointmentSystem.Data.Entities;

    public class AppDbContext : DbContext
    {
        public DbSet<SalesManager> SalesManagers { get; set; }
        public DbSet<Slot> Slots { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SalesManager>()
                .HasMany(sm => sm.Slots)
                .WithOne(s => s.SalesManager)
                .HasForeignKey(s => s.SalesManagerId)
                .OnDelete(DeleteBehavior.Cascade); 

            modelBuilder.Entity<SalesManager>()
                .Property(e => e.Languages)
                .HasColumnType("varchar[]");

            modelBuilder.Entity<SalesManager>()
                .Property(e => e.Products)
                .HasColumnType("varchar[]");

            modelBuilder.Entity<SalesManager>()
                .Property(e => e.CustomerRatings)
                .HasColumnType("varchar[]");

            modelBuilder.Entity<Slot>()
                .HasIndex(s => s.StartDate);

            base.OnModelCreating(modelBuilder);
        }
    }
}
