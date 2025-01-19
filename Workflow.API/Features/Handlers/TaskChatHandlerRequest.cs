namespace Workflow.API.Features.Microagent.Handlers
{
    using Workflow.API.Models.Request;
    using Workflow.API.Models.Response;
    using MediatR;

    public record TaskChatHandlerRequest : Message, IRequest<Chat>
    {
    }
}
