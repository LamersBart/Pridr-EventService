using Microsoft.EntityFrameworkCore;
using EventService.Models;

namespace EventService.Data;

public static class PrepDb
{
    public static  async Task PrepPopulation(IApplicationBuilder app, bool isProduction)
    {
        using (var serviceScope = app.ApplicationServices.CreateScope())
        {
            await SeedData(serviceScope.ServiceProvider.GetRequiredService<AppDbContext>(), isProduction);
        }
    }

    private static async Task SeedData(AppDbContext context, bool isProduction)
    {
        if(isProduction)
        {
            Console.WriteLine("--> Attemt to apply migrations...");
            try
            { 
                if (await context.Database.GetPendingMigrationsAsync() is { } migrations && migrations.Any())
                {
                    await context.Database.MigrateAsync();
                }
                // if (!await context.UserEvents.AnyAsync())
                // {
                //     Console.WriteLine("--> Seeding data");
                //     await context.UserEvents.AddRangeAsync(
                //         new UserEvent
                //         {
                //             Name = "Test-Event-1",
                //             Date = DateTime.UtcNow,
                //             ProfileIds = ["1", "2", "3"],
                //             CreatedBy = "KeycloakUserId-1",
                //         },
                //         new UserEvent
                //         {
                //             Name = "Test-Event-2",
                //             Date = DateTime.UtcNow,
                //             ProfileIds = ["4", "2", "6"],
                //             CreatedBy = "KeycloakUserId-2",
                //         });
                //     await context.SaveChangesAsync();
                // }
                // else
                // {
                //     Console.WriteLine("--> We already have data");
                // }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not run migrations: {ex.Message}");
            }
        }
    }
}