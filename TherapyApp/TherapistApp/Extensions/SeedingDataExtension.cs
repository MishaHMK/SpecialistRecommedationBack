using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Newtonsoft.Json;
using TherapyApp.Entities;
using TherapyApp.Helpers.Secrets;

namespace TherapyApp.Extensions;

public static class SeedingDataExtension
{
    private static TherapyDbContext _dbContext = null!;
    public static async Task SeedDataAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        _dbContext = scope.ServiceProvider.GetRequiredService<TherapyDbContext>();

        await SeedSpecializations();
        await SeedEmotions();
        await SeedUsers(app);
        await SeedDiary(app);

        await _dbContext.SaveChangesAsync();
    }

    private static async Task SeedSpecializations()
    {

        var specialities = new List<Speciality>()
        {
            new Speciality { Name = "Addiction Treatment", Value = 1.0 },
            new Speciality { Name = "Trauma Therapy", Value = 2.0 },
            new Speciality { Name = "Child Therapy", Value = 3.0 },
            new Speciality { Name = "Family Therapy", Value = 4.0 },
            new Speciality { Name = "Counseling", Value = 5.0 }
        };

        await _dbContext.Specialities.AddRangeAsync(specialities);

        await _dbContext.SaveChangesAsync();
    }

    private static async Task SeedUsers(WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
            var dbContext = scope.ServiceProvider.GetRequiredService<TherapyDbContext>();
            var adminConfig = app.Configuration.GetSection(nameof(AdminConfiguration)).Get<AdminConfiguration>();


            var roles = new[]
            {
                Roles.Therapist,
                Roles.Client,
                Roles.Admin
            };

            int roleId = 1;
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var idRole = new AppRole { Id = roleId.ToString(), Name = role };
                    await roleManager.CreateAsync(idRole);
                    roleId++;
                }
            }

            if (!dbContext.Users.Any())
            {
                var adminUser = new AppUser
                {
                    Id = Guid.NewGuid().ToString(),
                    FirstName = adminConfig.FirstName,
                    LastName = adminConfig.Surname,
                    Email = adminConfig.Email,
                    UserName = adminConfig.Login
                };

                var result = await userManager.CreateAsync(adminUser, adminConfig.Password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }

                var therapists = new List<AppUser>
                {
                    new AppUser { Id = Guid.NewGuid().ToString(), FirstName = "John", LastName = "Doe", Email = "john@example.com", UserName = "john1" },
                    new AppUser { Id = Guid.NewGuid().ToString(), FirstName = "Jane", LastName = "Smith", Email = "jane@example.com", UserName = "jane1"},
                    new AppUser { Id = Guid.NewGuid().ToString(), FirstName = "Alex", LastName = "Miller", Email = "alex@example.com", UserName = "alex1" },
                    new AppUser { Id = Guid.NewGuid().ToString(), FirstName = "Henry", LastName = "Jones", Email = "henry@example.com", UserName = "henry1"},
                    new AppUser { Id = Guid.NewGuid().ToString(), FirstName = "Max", LastName = "Fasber", Email = "max@example.com", UserName = "max1" }
                };

                foreach (var user in therapists)
                {
                    var res = await userManager.CreateAsync(user, "Therapist@123");

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Therapist");
                    }
                    else
                    {
                        foreach (var error in res.Errors)
                        {
                            Console.WriteLine($"Error creating user {user.UserName}: {error.Description}");
                        }
                    }
                }

                int i = 1;
                foreach (var user in therapists)
                {
                    var newTherapist = new Therapist
                    {
                        Introduction = $"Some introduction {i}",
                        SpecialityId = i,
                        UserId = user.Id
                    };

                    await dbContext.TherapistUsers.AddAsync(newTherapist);

                    i++;
                }

                var users = new List<AppUser>
                {
                    new AppUser { Id = Guid.NewGuid().ToString(),  FirstName = "Ann", LastName = "Foster", Email = "jane@example.com", UserName = "ann1" },
                    new AppUser { Id = Guid.NewGuid().ToString(), FirstName = "Bob", LastName = "Downey", Email = "bob@example.com", UserName = "bob1"}
                };

                foreach (var user in users)
                {
                    var res = await userManager.CreateAsync(user, "Client@123");

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Client");
                    }
                    else
                    {
                        foreach (var error in res.Errors)
                        {
                            Console.WriteLine($"Error creating user {user.UserName}: {error.Description}");
                        }
                    }
                }

                await dbContext.SaveChangesAsync();
            }
        }
    }

    private static async Task SeedEmotions()
    {

        List<Emotion>? emotions = new List<Emotion>()
        {
            new Emotion { Name = "Anxiety" },
            new Emotion { Name = "Depression" },
            new Emotion { Name = "Addiction"},
            new Emotion { Name = "Panic" },
            new Emotion { Name = "PTSD" },
            new Emotion { Name = "Anger" },
            new Emotion { Name = "Phobia" },
            new Emotion { Name = "Delirium" },
            new Emotion { Name = "Fatigue" },
            new Emotion { Name = "Functional Disorder" }
        };

        await _dbContext.Emotions.AddRangeAsync(emotions);

        await _dbContext.SaveChangesAsync();
    }

    private static async Task SeedDiary(WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var usersInBaseRole = await userManager.GetUsersInRoleAsync("Client");
            var diaries = new List<Diary>();
            int i = 1;

            foreach (var user in usersInBaseRole)
            {
                diaries.Add(new Diary { UserId = user.Id });
                i++;
            }

            await _dbContext.Diaries.AddRangeAsync(diaries);

            var newEntries = new List<DiaryEntry>()
            {
                new DiaryEntry()
                {
                    Description = String.Empty,
                    CreatedAt = DateTime.Now,
                    DiaryId = 1,
                    EmotionId = 1,
                    Value = 0.8
                },
                new DiaryEntry()
                {
                    Description = String.Empty,
                    CreatedAt = DateTime.Now,
                    DiaryId = 1,
                    EmotionId = 1,
                    Value = 0.8
                },
                new DiaryEntry()
                {
                    Description = String.Empty,
                    CreatedAt = DateTime.Now,
                    DiaryId = 1,
                    EmotionId = 2,
                    Value = 0.5
                },
                new DiaryEntry()
                {
                    Description = String.Empty,
                    CreatedAt = DateTime.Now,
                    DiaryId = 2,
                    EmotionId = 5,
                    Value = 1
                },
                new DiaryEntry()
                {
                    Description = String.Empty,
                    CreatedAt = DateTime.Now,
                    DiaryId = 2,
                    EmotionId = 5,
                    Value = 0.8
                },
                new DiaryEntry()
                {
                    Description = String.Empty,
                    CreatedAt = DateTime.Now,
                    DiaryId = 2,
                    EmotionId = 6,
                    Value = 0.7
                }
            };

            await _dbContext.DiaryEntries.AddRangeAsync(newEntries);
        }

    }
}
