namespace Workflow.API.Features.Microagent.Handlers
{
    using Workflow.API.Models.Request;
    using Workflow.API.Models.Response;
    using MediatR;

    public record ConversationHandlerRequest : Message, IRequest<Chat>
    {
    }
}
