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

        AddCommonResponses(operation, context.ApiDescription.HttpMethod, moduleName);
    }

    private static string BuildSummary(string? method, string? relativePath)
    {
        return $"{method ?? "HTTP"} /{relativePath ?? string.Empty}".Trim();
    }

    private static void AddCommonResponses(OpenApiOperation operation, string? method, string moduleName)
    {
        if (method == "POST")
        {
            operation.Responses.TryAdd("201", new OpenApiResponse { Description = "Created." });
        }

        if (method == "GET" || method == "PUT" || method == "POST")
        {
            operation.Responses.TryAdd("200", new OpenApiResponse { Description = "Request completed successfully." });
        }

        if (method == "DELETE")
        {
            operation.Responses.TryAdd("204", new OpenApiResponse { Description = "Deleted successfully." });
        }

        operation.Responses.TryAdd("400", new OpenApiResponse { Description = "Validation failed." });
        operation.Responses.TryAdd("429", new OpenApiResponse { Description = "Too many requests." });
        operation.Responses.TryAdd("500", new OpenApiResponse { Description = "Unexpected server error." });

        if (moduleName != "Auth" && moduleName != "Health")
        {
            operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Authentication is required or the token is invalid." });
            operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Authenticated user cannot access this resource." });
        }

        if (method is "GET" or "PUT" or "DELETE")
        {
            operation.Responses.TryAdd("404", new OpenApiResponse { Description = "Resource or endpoint was not found." });
        }

        operation.Responses.TryAdd("409", new OpenApiResponse { Description = "Request conflicts with current resource state." });
    }
}
