
namespace Inventory.API.Setup;

using Microsoft.AspNetCore.Http;
using MediatR;
using Inventory.API.Features.Microagent.Handlers;
using Inventory.API.Models.Response;

public static class WebApplicationExtensions
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        app.MapPost("/converse", Converse)
            .WithName("converse")
            .WithTags("converse")
            .Produces<Models.Response.Chat>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<IResult>(StatusCodes.Status400BadRequest);

    }

    private static async Task<Chat> Converse(IMediator mediator, ConversationHandlerRequest req) => await mediator.Send(req);
    
}
   