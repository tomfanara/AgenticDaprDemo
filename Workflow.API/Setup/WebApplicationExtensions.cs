
namespace Workflow.API.Setup;

using Microsoft.AspNetCore.Http;
using MediatR;
using Workflow.API.Features.Microagent.Handlers;
using Workflow.API.Models.Response;
using Microsoft.AspNetCore.Mvc;

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
        app.MapPost("/groupchatinitializer", GroupChatInitializer)
            .WithName("groupchatinitializer")
            .WithTags("groupchatinititalizer")
            .Produces<Models.Response.Chat>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<IResult>(StatusCodes.Status400BadRequest);
        app.MapPost("/groupchatraiseevent", GroupChatRaiseEvent)
        .Accepts<string>("application/json")
        .WithName("groupchatraiseevent")
        .WithTags("groupchatraiseevent")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }

    private static async Task<Chat> TaskChat(IMediator mediator, [FromBody] ConversationHandlerRequest req) => await mediator.Send(req);

    private static async Task<Chat> GroupChatInitializer(IMediator mediator, [FromBody] GroupChatHandlerRequest req) => await mediator.Send(req);

    private static async void GroupChatRaiseEvent(IMediator mediator, [FromBody] RaiseEventHandlerRequest req) => await mediator.Send(req);


}
   