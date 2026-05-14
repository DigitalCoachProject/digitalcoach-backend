using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DigitalCoach.Infrastructure.Persistence.Context;

public sealed class DigitalCoachDbContextFactory : IDesignTimeDbContextFactory<DigitalCoachDbContext>
{
    public DigitalCoachDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var apiProjectPath = ResolveApiProjectPath();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiProjectPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

        var optionsBuilder = new DbContextOptionsBuilder<DigitalCoachDbContext>();
        optionsBuilder.UseSqlServer(connectionString, sqlServer =>
        {
            sqlServer.MigrationsAssembly(typeof(DigitalCoachDbContext).Assembly.FullName);
            sqlServer.EnableRetryOnFailure();
        });

        return new DigitalCoachDbContext(optionsBuilder.Options);
    }

    private static string ResolveApiProjectPath()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "appsettings.json"))
                && string.Equals(current.Name, "DigitalCoach.Api", StringComparison.OrdinalIgnoreCase))
            {
                return current.FullName;
            }

            var candidate = Path.Combine(current.FullName, "src", "DigitalCoach.Api");
            if (File.Exists(Path.Combine(candidate, "appsettings.json")))
            {
                return candidate;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate src/DigitalCoach.Api for design-time DbContext configuration.");
    }
}
