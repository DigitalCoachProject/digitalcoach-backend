using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using DigitalCoach.Api.Common;
using DigitalCoach.Api.Middleware;
using DigitalCoach.Api.Swagger;
using DigitalCoach.Application;
using DigitalCoach.Infrastructure;
using DigitalCoach.Infrastructure.Persistence.Context;
using DigitalCoach.Infrastructure.Services;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DataProtectionKeys")));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpsRedirection(options =>
{
    var httpsPort = builder.Configuration.GetValue<int?>("HttpsRedirection:HttpsPort");
    if (httpsPort.HasValue)
    {
        options.HttpsPort = httpsPort.Value;
    }
});

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT configuration section is missing.");

if (string.IsNullOrWhiteSpace(jwtOptions.SigningKey) || Encoding.UTF8.GetByteCount(jwtOptions.SigningKey) < 32)
{
    throw new InvalidOperationException("JWT signing key must contain at least 32 bytes. Configure Jwt__Key or Jwt__SecretKey.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            NameClaimType = JwtRegisteredClaimNames.Email
        };
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                var response = ApiResponse<object>.Failure("Authentication is required or the token is invalid.");
                await JsonSerializer.SerializeAsync(context.Response.Body, response, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            },
            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";

                var response = ApiResponse<object>.Failure("You do not have permission to access this resource.");
                await JsonSerializer.SerializeAsync(context.Response.Body, response, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(
                x => ToCamelCaseKey(x.Key),
                x => x.Value!.Errors.Select(error => error.ErrorMessage).ToArray());

        return new BadRequestObjectResult(ApiResponse<object>.Failure("Validation failed.", errors));
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.DescribeAllParametersInCamelCase();
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DigitalCoach API",
        Version = "v1"
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter JWT bearer token only. Example: eyJhbGciOiJIUzI1NiIs...",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = JwtBearerDefaults.AuthenticationScheme
        }
    };

    options.AddSecurityDefinition("Bearer", securityScheme);
    options.OperationFilter<AuthorizeOperationFilter>();
    options.OperationFilter<PaginationQueryParameterOperationFilter>();
    options.OperationFilter<ModuleOperationFilter>();
});

var app = builder.Build();

app.Logger.LogInformation(
    "DigitalCoach API starting. Environment: {EnvironmentName}. ContentRoot: {ContentRoot}.",
    app.Environment.EnvironmentName,
    app.Environment.ContentRootPath);

await ApplyDatabaseMigrationsAsync(app);

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseStatusCodePages(async statusCodeContext =>
{
    var httpContext = statusCodeContext.HttpContext;
    if (httpContext.Response.HasStarted || httpContext.Response.StatusCode < StatusCodes.Status400BadRequest)
    {
        return;
    }

    httpContext.Response.ContentType = "application/json";
    var message = httpContext.Response.StatusCode switch
    {
        StatusCodes.Status404NotFound => "The requested endpoint was not found.",
        StatusCodes.Status405MethodNotAllowed => "The HTTP method is not allowed for this endpoint.",
        _ => "The request could not be processed."
    };

    var response = ApiResponse<object>.Failure(message);
    await JsonSerializer.SerializeAsync(httpContext.Response.Body, response, new JsonSerializerOptions(JsonSerializerDefaults.Web));
});

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Docker"))
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static async Task ApplyDatabaseMigrationsAsync(WebApplication app)
{
    const int maxAttempts = 12;
    var delay = TimeSpan.FromSeconds(5);

    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DigitalCoachDbContext>();
            var configuredMigrations = dbContext.Database.GetMigrations().ToList();

            app.Logger.LogInformation(
                "Checking database and applying pending EF Core migrations. Configured migrations: {Migrations}",
                configuredMigrations.Count == 0 ? "(none)" : string.Join(", ", configuredMigrations));

            await dbContext.Database.MigrateAsync();
            app.Logger.LogInformation("Database migration check completed successfully.");
            return;
        }
        catch (Exception exception) when (attempt < maxAttempts)
        {
            app.Logger.LogWarning(
                "Database migration attempt {Attempt}/{MaxAttempts} failed with {ExceptionType}: {ExceptionMessage}. Retrying in {DelaySeconds} seconds.",
                attempt,
                maxAttempts,
                exception.GetType().Name,
                exception.Message,
                delay.TotalSeconds);

            await Task.Delay(delay);
        }
        catch (Exception exception)
        {
            app.Logger.LogCritical(
                exception,
                "Database migration failed after {MaxAttempts} attempts. API startup cannot continue safely.",
                maxAttempts);
            throw;
        }
    }
}

static string ToCamelCaseKey(string key)
{
    if (string.IsNullOrWhiteSpace(key) || key == "$")
    {
        return key;
    }

    var segments = key.Split('.');
    for (var i = 0; i < segments.Length; i++)
    {
        var segment = segments[i];
        if (string.IsNullOrWhiteSpace(segment) || !char.IsUpper(segment[0]))
        {
            continue;
        }

        segments[i] = char.ToLowerInvariant(segment[0]) + segment[1..];
    }

    return string.Join('.', segments);
}
