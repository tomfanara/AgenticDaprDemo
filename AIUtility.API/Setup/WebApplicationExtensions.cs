
namespace AIUtility.API.Setup;

using Microsoft.AspNetCore.Http;
using MediatR;
using AIUtility.API.Features.Microagent.Handlers;
using AIUtility.API.Models.Response;

public static class WebApplicationExtensions
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        app.MapPost("/requestrewrite", RequestRewrite)
            .WithName("requestrewrite")
            .WithTags("requestrewrite")
            .Produces<Models.Response.Chat>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<IResult>(StatusCodes.Status400BadRequest);
        app.MapPost("/responsewrite", ResponseRewrite)
        .Accepts<string>("application/json")
        .WithName("Endpoint2")
        .WithTags("MyEndpoints2")
        .Produces(StatusCodes.Status500InternalServerError)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }

    private static async Task<Chat> RequestRewrite(IMediator mediator, RequestRewriteHandlerRequest req) => await mediator.Send(req);

    private static async Task<Rewrite> ResponseRewrite(IMediator mediator, ResultRewriteHandlerRequest req) => await mediator.Send(req);

}
   