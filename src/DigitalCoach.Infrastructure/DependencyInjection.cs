using DigitalCoach.Application.Abstractions.Repositories;
using DigitalCoach.Application.Abstractions.Services;
using DigitalCoach.Infrastructure.Persistence.Context;
using DigitalCoach.Infrastructure.Repositories;
using DigitalCoach.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalCoach.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

        services.AddDbContext<DigitalCoachDbContext>(options =>
            options.UseSqlServer(connectionString, sqlServer =>
            {
                sqlServer.MigrationsAssembly(typeof(DigitalCoachDbContext).Assembly.FullName);
                sqlServer.EnableRetryOnFailure();
            }));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        services.AddScoped<IHabitRepository, HabitRepository>();
        services.AddScoped<IHabitLogRepository, HabitLogRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<ITaskHistoryRepository, TaskHistoryRepository>();
        services.AddScoped<IDailyStateRepository, DailyStateRepository>();
        services.AddScoped<IProductivityOverviewRepository, ProductivityOverviewRepository>();
        services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IHabitService, HabitService>();
        services.AddScoped<IDailyStateService, DailyStateService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<IRecommendationService, RecommendationService>();

        return services;
    }
}
