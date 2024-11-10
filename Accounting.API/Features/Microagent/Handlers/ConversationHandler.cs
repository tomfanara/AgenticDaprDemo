namespace Accounting.API.Features.Microagent.Handlers
{
    using Accounting.API.Models.Response;
    using MediatR;

    public class ConversationHandler()
        : IRequestHandler<ConversationHandlerRequest, Chat>
    {
       
        public async Task<Chat> Handle(ConversationHandlerRequest request, CancellationToken cancellationToken)
        {
            Chat chat = new Chat { Conversation = "Good Bye" };
            return chat;
        }
    }
}
