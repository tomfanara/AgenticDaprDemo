
namespace Workflow.API.Setup;

using Microsoft.AspNetCore.Http;
using MediatR;
using Workflow.API.Features.Microagent.Handlers;
using Workflow.API.Models.Response;

public static class WebApplicationExtensions
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        app.MapGet("/initialize", Endpoint)
        .WithName("Endpoint")
        .WithTags("MyEndpoints")
        .Produces(StatusCodes.Status500InternalServerError)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
        app.MapPost("/raiseevent", Endpoint2)
        .Accepts<string>("application/json")
        .WithName("Endpoint2")
        .WithTags("MyEndpoints2")
        .Produces(StatusCodes.Status500InternalServerError)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> Endpoint(IMediator mediator)
    {
        var result = await mediator.Send(new InitializeWorkflowCommand
        {

        });
        return Results.Ok();
    }

    private static async Task<IResult> Endpoint2(IMediator mediator, string message)
    {
        var result = await mediator.Send(new RaiseEventWorkflowCommand
        {
            message = message,
        });
        return Results.Ok();
    }

}
   