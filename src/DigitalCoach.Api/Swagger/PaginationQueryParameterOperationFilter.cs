using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DigitalCoach.Api.Swagger;

public sealed class PaginationQueryParameterOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters is null || operation.Parameters.Count == 0)
        {
            return;
        }

        var relativePath = context.ApiDescription.RelativePath ?? string.Empty;
        foreach (var parameter in operation.Parameters)
        {
            ApplyCommonMetadata(parameter);
            ApplySortByMetadata(parameter, relativePath);
        }
    }

    private static void ApplyCommonMetadata(OpenApiParameter parameter)
    {
        switch (parameter.Name)
        {
            case "page":
                parameter.Description = "Page number. Minimum value is 1.";
                parameter.Schema.Default = new OpenApiInteger(1);
                parameter.Example = new OpenApiInteger(1);
                break;
            case "pageSize":
                parameter.Description = "Number of items per page. Allowed range is 1-100.";
                parameter.Schema.Default = new OpenApiInteger(20);
                parameter.Example = new OpenApiInteger(20);
                break;
            case "sortDescending":
                parameter.Description = "Sort descending when true; ascending when false.";
                parameter.Schema.Default = new OpenApiBoolean(false);
                parameter.Example = new OpenApiBoolean(false);
                break;
            case "Page":
                parameter.Name = "page";
                parameter.Description = "Page number. Minimum value is 1.";
                parameter.Schema.Default = new OpenApiInteger(1);
                parameter.Example = new OpenApiInteger(1);
                break;
            case "PageSize":
                parameter.Name = "pageSize";
                parameter.Description = "Number of items per page. Allowed range is 1-100.";
                parameter.Schema.Default = new OpenApiInteger(20);
                parameter.Example = new OpenApiInteger(20);
                break;
            case "SortDescending":
                parameter.Name = "sortDescending";
                parameter.Description = "Sort descending when true; ascending when false.";
                parameter.Schema.Default = new OpenApiBoolean(false);
                parameter.Example = new OpenApiBoolean(false);
                break;
        }
    }

    private static void ApplySortByMetadata(OpenApiParameter parameter, string relativePath)
    {
        if (!string.Equals(parameter.Name, "sortBy", StringComparison.Ordinal)
            && !string.Equals(parameter.Name, "SortBy", StringComparison.Ordinal))
        {
            return;
        }

        var values = GetSortFields(relativePath);
        if (values.Length == 0)
        {
            return;
        }

        parameter.Name = "sortBy";
        parameter.Description = $"Sort field. Allowed values: {string.Join(", ", values)}.";
        parameter.Example = new OpenApiString(values[0]);
    }

    private static string[] GetSortFields(string relativePath)
    {
        return relativePath switch
        {
            "api/habits" => ["name", "created_at", "difficulty"],
            "api/tasks" => ["created_at", "planned_date", "deadline", "priority", "difficulty"],
            "api/notifications" => ["created_at", "priority", "is_read"],
            "api/recommendations" => ["created_at", "priority", "is_read"],
            _ => []
        };
    }
}
