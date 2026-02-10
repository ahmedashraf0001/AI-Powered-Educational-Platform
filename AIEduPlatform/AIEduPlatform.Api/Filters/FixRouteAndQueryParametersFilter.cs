using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.RegularExpressions;

namespace AIEduPlatform.Api.Filters
{
    public class FixRouteAndQueryParametersFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var httpMethod = context.ApiDescription.HttpMethod;
            var relativePath = context.ApiDescription.RelativePath ?? string.Empty;

            var routeParamNames = Regex.Matches(relativePath, @"\{(\w+)\}")
                .Select(m => m.Groups[1].Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Ensure route params are listed as path parameters
            foreach (var paramName in routeParamNames)
            {
                var existing = operation.Parameters?
                    .FirstOrDefault(p => string.Equals(p.Name, paramName, StringComparison.OrdinalIgnoreCase));

                if (existing == null)
                {
                    operation.Parameters ??= [];
                    operation.Parameters.Add(new OpenApiParameter
                    {
                        Name = paramName,
                        In = ParameterLocation.Path,
                        Required = true,
                        Schema = new OpenApiSchema { Type = JsonSchemaType.String }
                    });
                }
                else if (existing.In != ParameterLocation.Path)
                {
                    // Replace with a corrected copy since IOpenApiParameter properties are read-only
                    operation.Parameters.Remove(existing);
                    operation.Parameters.Add(new OpenApiParameter
                    {
                        Name = existing.Name,
                        In = ParameterLocation.Path,
                        Required = true,
                        Schema = existing.Schema ?? new OpenApiSchema { Type = JsonSchemaType.String }
                    });
                }
            }

            if (operation.RequestBody != null)
            {
                var isGetOrHead = string.Equals(httpMethod, "GET", StringComparison.OrdinalIgnoreCase) ||
                                  string.Equals(httpMethod, "HEAD", StringComparison.OrdinalIgnoreCase);

                // Collect all body property names
                var bodyPropNames = operation.RequestBody.Content.Values
                    .Where(c => c.Schema?.Properties != null)
                    .SelectMany(c => c.Schema.Properties.Select(p => p.Key))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                // Check if every body property is already covered by a route parameter
                var allCoveredByRoute = bodyPropNames.Count > 0 &&
                    bodyPropNames.All(name => routeParamNames.Contains(name));

                if (isGetOrHead)
                {
                    // For GET/HEAD: move non-route body properties to query params
                    foreach (var content in operation.RequestBody.Content.Values)
                    {
                        var schema = content.Schema;
                        if (schema?.Properties != null)
                        {
                            foreach (var prop in schema.Properties)
                            {
                                if (routeParamNames.Contains(prop.Key))
                                    continue;

                                var alreadyExists = operation.Parameters?
                                    .Any(p => string.Equals(p.Name, prop.Key, StringComparison.OrdinalIgnoreCase)) ?? false;

                                if (!alreadyExists)
                                {
                                    operation.Parameters ??= [];
                                    operation.Parameters.Add(new OpenApiParameter
                                    {
                                        Name = prop.Key,
                                        In = ParameterLocation.Query,
                                        Required = false,
                                        Schema = prop.Value
                                    });
                                }
                            }
                        }
                    }

                    operation.RequestBody = null;
                }
                else if (allCoveredByRoute)
                {
                    // For POST/PUT/DELETE: remove body when all properties are route params
                    operation.RequestBody = null;
                }
            }
        }
    }
}
