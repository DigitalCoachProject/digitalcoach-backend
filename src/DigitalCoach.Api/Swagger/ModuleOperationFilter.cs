using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DigitalCoach.Api.Swagger;

public sealed class ModuleOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var controllerName = context.ApiDescription.ActionDescriptor.RouteValues["controller"] ?? string.Empty;
        var moduleName = controllerName switch
        {
            "Auth" => "Auth",
            "Habits" => "Habits",
            "Tasks" => "Tasks",
            "DailyState" => "DailyStates",
            "Analytics" => "Analytics",
            "Recommendations" => "Recommendations",
            "Notifications" => "Notifications",
            "Health" => "Health",
            _ => controllerName
        };

        operation.Tags = [new OpenApiTag { Name = moduleName }];
        operation.Summary ??= BuildSummary(context.ApiDescription.HttpMethod, context.ApiDescription.RelativePath);
        operation.Description ??= $"DigitalCoach {moduleName} endpoint.";
    }

    private static string BuildSummary(string? method, string? relativePath)
    {
        return $"{method ?? "HTTP"} /{relativePath ?? string.Empty}".Trim();
    }
}
