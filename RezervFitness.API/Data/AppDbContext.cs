using RezervFitness.API.Models;
using Microsoft.EntityFrameworkCore;
namespace RezervFitness.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Business> Businesses => Set<Business>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Package> Packages => Set<Package>();
        public DbSet<TimetableSchedule> TimetableSchedules =>
            Set<TimetableSchedule>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<WaitlistEntry> WaitlistEntries =>
            Set<WaitlistEntry>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Business>()
                .HasIndex(b => b.Name);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Package>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Package>()
            .HasOne(p => p.Business)
            .WithMany()
            .HasForeignKey(p => p.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TimetableSchedule>()
                .HasOne(s => s.Business)
                .WithMany()
                .HasForeignKey(s => s.BusinessId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
            .HasOne(b => b.TimetableSchedule)
            .WithMany()
            .HasForeignKey(b => b.TimetableScheduleId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Package)
                .WithMany()
                .HasForeignKey(b => b.PackageId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WaitlistEntry>()
                .HasOne(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WaitlistEntry>()
            .HasOne(w => w.TimetableSchedule)
            .WithMany()
            .HasForeignKey(w => w.TimetableScheduleId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WaitlistEntry>()
                .HasOne(w => w.Package)
                .WithMany()
                .HasForeignKey(w => w.PackageId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasIndex(b => new
                {
                    b.UserId,
                    b.TimetableScheduleId,
                    b.Status
                });

            modelBuilder.Entity<WaitlistEntry>()
            .HasIndex(w => new
            {
                w.TimetableScheduleId,
                w.Status,
                w.JoinedAt,
                w.Id
            });

            modelBuilder.Entity<Booking>()
            .HasIndex(b => new
            {
                b.TimetableScheduleId,
                b.Status
            });

            modelBuilder.Entity<WaitlistEntry>()
            .HasIndex(w => new
            {
                w.UserId,
                w.TimetableScheduleId,
                w.Status
            });

        }
    }

}
