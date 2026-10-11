using RezervFitness.API.Models;

namespace RezervFitness.API.Data
{
    public class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if ( context.Businesses.Any())
                return;

            var businessA = new Business
            {
                Name = "Rezerv Fitness",
                Description = "Main fitness studio",
                IsActive = true
            };

            var businessB = new Business
            {
                Name = "Rezerv Yoga Studio",
                Description = "Yoga and wellness studio",
                IsActive = true
            };

            var users = Enumerable.Range(1, 10)
           .Select(i => new User
           {
               FullName = $"Test User {i}",
               Email = $"user{i}@example.com"
           })
           .ToList();

            context.Businesses.AddRange(businessA, businessB);
            context.Users.AddRange(users);

            await context.SaveChangesAsync();

            var today = DateTime.Today;

            var schedules = new List<TimetableSchedule>
        {
            new() { BusinessId = businessA.Id, ClassName = "Yoga Class", InstructorName = "John Doe", StartTime = today.AddDays(1).AddHours(10), EndTime = today.AddDays(1).AddHours(11), AvailableSlots = 15 },
            new() { BusinessId = businessA.Id, ClassName = "HIIT Training", InstructorName = "Jane Smith", StartTime = today.AddDays(1).AddHours(12), EndTime = today.AddDays(1).AddHours(13), AvailableSlots = 10 },
            new() { BusinessId = businessA.Id, ClassName = "Pilates", InstructorName = "Alex Lee", StartTime = today.AddDays(1).AddHours(14), EndTime = today.AddDays(1).AddHours(15), AvailableSlots = 8 },
            new() { BusinessId = businessA.Id, ClassName = "Spin Class", InstructorName = "Sam Taylor", StartTime = today.AddDays(2).AddHours(9), EndTime = today.AddDays(2).AddHours(10), AvailableSlots = 12 },
            new() { BusinessId = businessA.Id, ClassName = "Strength Training", InstructorName = "Chris Brown", StartTime = today.AddDays(2).AddHours(11), EndTime = today.AddDays(2).AddHours(12), AvailableSlots = 5 },
            new() { BusinessId = businessB.Id, ClassName = "Morning Yoga", InstructorName = "Emily Clark", StartTime = today.AddDays(1).AddHours(10), EndTime = today.AddDays(1).AddHours(11), AvailableSlots = 10 },
            new() { BusinessId = businessB.Id, ClassName = "Meditation", InstructorName = "David Wilson", StartTime = today.AddDays(1).AddHours(12), EndTime = today.AddDays(1).AddHours(13), AvailableSlots = 6 },
            new() { BusinessId = businessB.Id, ClassName = "Vinyasa Flow", InstructorName = "Sarah Davis", StartTime = today.AddDays(2).AddHours(15), EndTime = today.AddDays(2).AddHours(16), AvailableSlots = 8 },
            new() { BusinessId = businessA.Id, ClassName = "Boxing Fitness", InstructorName = "Mike Johnson", StartTime = today.AddDays(3).AddHours(16), EndTime = today.AddDays(3).AddHours(17), AvailableSlots = 1 },
            new() { BusinessId = businessB.Id, ClassName = "Restorative Yoga", InstructorName = "Olivia Martin", StartTime = today.AddDays(3).AddHours(18), EndTime = today.AddDays(3).AddHours(19), AvailableSlots = 1 }
        };

            context.TimetableSchedules.AddRange(schedules);

            var packages = new List<Package>();

            for (var i = 0; i < users.Count; i++)
            {
                packages.Add(new Package
                {
                    UserId = users[i].Id,
                    BusinessId = i % 2 == 0 ? businessA.Id : businessB.Id,
                    TotalCredits = 10,
                    RemainingCredits = 10,
                    ExpiryDate = DateTime.UtcNow.AddMonths(1)
                });
            }

            packages.Add(new Package
            {
                UserId = users[0].Id,
                BusinessId = businessA.Id,
                TotalCredits = 10,
                RemainingCredits = 10,
                ExpiryDate = DateTime.UtcNow.AddDays(-1)
            });

            context.Packages.AddRange(packages);

            await context.SaveChangesAsync();
        }

    }
}
