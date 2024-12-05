using Microsoft.EntityFrameworkCore;
using EventService.Models;

namespace EventService.Data;

public class prepDb
{
    public static void PrepPopulation(IApplicationBuilder app, bool isProduction)
    {
        using (var serviceScope = app.ApplicationServices.CreateScope())
        {
            SeedData(serviceScope.ServiceProvider.GetRequiredService<AppDbContext>(), isProduction);
        }
    }

    private static void SeedData(AppDbContext context, bool isProduction)
    {
        if(isProduction)
        {
            Console.WriteLine("--> Attemt to apply migrations...");
            try
            { 
                context.Database.Migrate();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not run migrations: {ex.Message}");
            }
        }

        if (!context.UserEvents.Any())
        {
            Console.WriteLine("--> Seeding data");
            context.UserEvents.AddRange(
                new UserEvent
                {
                    Name = "Test-Event-1",
                    Date = DateTime.UtcNow,
                    ProfileIds = [1, 2, 3],
                    CreatedBy = "KeycloakUserId-1",
                },
                new UserEvent
                {
                    Name = "Test-Event-2",
                    Date = DateTime.UtcNow,
                    ProfileIds = [4, 2, 6],
                    CreatedBy = "KeycloakUserId-2",
                });
            context.SaveChanges();
        }
        else
        {
            Console.WriteLine("--> We already have data");
        }
    }
}