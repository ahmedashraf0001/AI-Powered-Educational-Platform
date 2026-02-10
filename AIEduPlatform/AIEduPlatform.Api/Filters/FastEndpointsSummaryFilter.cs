using FastEndpoints;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AIEduPlatform.Api.Filters;

public class FastEndpointsSummaryFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;
        if (metadata == null) return;

        var endpointDef = metadata.OfType<EndpointDefinition>().FirstOrDefault();
        if (endpointDef?.EndpointSummary is not { } summary) return;

        if (!string.IsNullOrEmpty(summary.Summary))
            operation.Summary = summary.Summary;

        if (!string.IsNullOrEmpty(summary.Description))
            operation.Description = summary.Description;

        foreach (var (statusCode, description) in summary.Responses)
        {
            var key = statusCode.ToString();
            if (operation.Responses.TryGetValue(key, out var response))
            {
                response.Description = description;
            }
        }
    }
}
