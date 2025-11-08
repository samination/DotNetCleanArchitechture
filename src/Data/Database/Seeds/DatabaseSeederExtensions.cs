using System;
using System.Threading;
using System.Threading.Tasks;
using Data.Database.Seeds;
using Data.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Data.Database;

public static class DatabaseSeederExtensions
{
    public static async Task SeedDatabaseAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        await context.Database.EnsureCreatedAsync(cancellationToken);
        await SeedDefaultIdentityAsync(userManager, roleManager, cancellationToken);

        if (await context.Categories.AnyAsync(cancellationToken))
        {
            return;
        }

        await context.Categories.AddRangeAsync(CategoryDataFixture.Categories, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedDefaultIdentityAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        CancellationToken cancellationToken)
    {
        const string adminRoleName = "Administrator";
        const string adminEmail = "admin@local";
        const string adminPassword = "Admin123!";

        if (!await roleManager.RoleExistsAsync(adminRoleName))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(adminRoleName));
        }

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is not null)
        {
            return;
        }

        adminUser = new ApplicationUser
        {
            Email = adminEmail,
            UserName = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, adminRoleName);
        }
    }
}

