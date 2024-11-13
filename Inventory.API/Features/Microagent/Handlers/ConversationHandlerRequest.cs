namespace Inventory.API.Features.Microagent.Handlers
{
    using Inventory.API.Models.Request;
    using Inventory.API.Models.Response;
    using MediatR;

    public record ConversationHandlerRequest : Message, IRequest<Chat>
    {
    }
}
