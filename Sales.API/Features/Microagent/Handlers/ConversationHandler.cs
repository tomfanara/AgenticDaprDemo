namespace Sales.API.Features.Microagent.Handlers;

using Dapr.Actors;
using Dapr.Actors.Client;
using MediatR;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Sales.API.Features.Microagent.Actors;
using Sales.API.Models.Response;

public class ConversationHandler()
: IRequestHandler<ConversationHandlerRequest, Chat>
{
    public async Task<Chat> Handle(ConversationHandlerRequest request, CancellationToken cancellationToken)
    {
        Guid guid = Guid.NewGuid();

        // Convert the GUID to a string and take the first 8 characters
        string shortGuid = guid.ToString().Substring(0, 8);
        var salesActorType = "SalesActor";

        var actorId= new ActorId(shortGuid);

        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage(request.Messages);

        // Create the local proxy by using the same interface that the service implements.
        // You need to provide the type and id so the actor can be located. 
        // If the actor matching this id does not exist, it will be created
        var proxySales = ActorProxy.Create<ISales>(actorId, salesActorType);

        var sales = proxySales.GetSales(request.Messages);

        chatHistory.Add(new ChatMessageContent(AuthorRole.Assistant, sales.Result.Conversation));

        var chatHistoryData = new SalesChatHistoryData()
        {
            ChatHistory = chatHistory
        };

        var salesChatHistory = await proxySales.SaveChatHistoryAsync(chatHistoryData);

        return sales.Result;
    }

    
}
