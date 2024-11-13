namespace Sales.API.Features.Microagent.Handlers
{
    using Sales.API.Models.Request;
    using Sales.API.Models.Response;
    using MediatR;

    public record ConversationHandlerRequest : Message, IRequest<Chat>
    {
    }
}
