
namespace Memory.API.Setup;

using Microsoft.AspNetCore.Http;
using MediatR;
using Memory.API.Features.SemanticKernel;
using Microsoft.OpenApi.Models;
using System.Text;

public static class WebApplicationExtensions
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        app.MapPost("Query",
        async (ISemanticKernelService semanticKernelService, string query) =>
        {
            var result = await semanticKernelService.Query(query);
            return TypedResults.Ok(result);
        })
        .WithOpenApi(operation =>
        {
            operation.Summary = "Query Transcriptions";
            operation.Description = "Query Transcriptions";
            operation.Tags = new List<OpenApiTag> { new() { Name = "AI" } };

            return operation;
        });

        app.MapPost("/upload",
            async (IFormFile file) =>
            {
              await using var stream = file.OpenReadStream();
              var buffer = new byte[stream.Length];
              await stream.ReadAsync(buffer);
              var text = Encoding.UTF8.GetString(buffer);

              var semanticKernelService = app.Services.GetRequiredService<ISemanticKernelService>();
              await semanticKernelService.ImportText(text);

              return TypedResults.Ok();
        })
         .DisableAntiforgery();
    }

}
   