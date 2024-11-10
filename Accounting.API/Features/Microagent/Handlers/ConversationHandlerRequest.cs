namespace Accounting.API.Features.Microagent.Handlers
{
    using Accounting.API.Models.Request;
    using Accounting.API.Models.Response;
    using MediatR;

    public record ConversationHandlerRequest : Message, IRequest<Chat>
    {
    }
}
