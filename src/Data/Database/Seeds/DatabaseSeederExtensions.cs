using System;
using System.Threading;
using System.Threading.Tasks;
using Data.Database.Seeds;
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

        if (await context.Categories.AnyAsync(cancellationToken))
        {
            return;
        }

        await context.Categories.AddRangeAsync(CategoryDataFixture.Categories, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}

