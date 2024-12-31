namespace AIUtility.API.Features.Microagent.Handlers
{
    using AIUtility.API.Models.Request;
    using AIUtility.API.Models.Response;
    using MediatR;

    public record RequestRewriteHandlerRequest : Message, IRequest<Chat>
    {
    }
}
