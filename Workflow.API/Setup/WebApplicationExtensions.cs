
namespace Workflow.API.Setup;

using Microsoft.AspNetCore.Http;
using MediatR;
using Workflow.API.Features.Microagent.Handlers;
using Workflow.API.Models.Response;

public static class WebApplicationExtensions
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        app.MapPost("/taskchat", TaskChat)
            .WithName("taskchat")
            .WithTags("taskchat")
            .Produces<Models.Response.Chat>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<IResult>(StatusCodes.Status400BadRequest);
        app.MapPost("/groupchat", GroupChat)
            .WithName("groupchat")
            .WithTags("groupchat")
            .Produces<Models.Response.Chat>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<IResult>(StatusCodes.Status400BadRequest);
        app.MapPost("/raiseevent", RaiseEvent)
        .Accepts<string>("application/json")
        .WithName("Endpoint2")
        .WithTags("MyEndpoints2")
        .Produces(StatusCodes.Status500InternalServerError)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }

    private static async Task<Chat> TaskChat(IMediator mediator, ConversationHandlerRequest req) => await mediator.Send(req);

    private static async Task<Chat> GroupChat(IMediator mediator, ConversationHandlerRequest req) => await mediator.Send(req);

    private static async Task<IResult> RaiseEvent(IMediator mediator, string message)
    {
        var result = await mediator.Send(new RaiseEventWorkflowCommand
        {
            message = message,
        });
        return Results.Ok();
    }

}
   