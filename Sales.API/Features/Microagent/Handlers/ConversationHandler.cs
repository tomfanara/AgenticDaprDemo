namespace Sales.API.Features.Microagent.Handlers
{
    using Sales.API.Models.Response;
    using MediatR;
    using Microsoft.SemanticKernel;
    using Microsoft.SemanticKernel.ChatCompletion;
    using System.Text.Json.Nodes;
    using System;
    
    public class ConversationHandler()
        : IRequestHandler<ConversationHandlerRequest, Chat>
    {
        // Create a kernel with OpenAI chat completion
        // Warning due to the experimental state of some Semantic Kernel SDK features.
        #pragma warning disable SKEXP0070
        Kernel kernel = Kernel.CreateBuilder()
                            .AddOllamaChatCompletion(
                                modelId: "phi3.5",
                                endpoint: new Uri("http://localhost:11434"))
                            .Build();


        public async Task<Chat> Handle(ConversationHandlerRequest request, CancellationToken cancellationToken)
        {
            var aiChatService = kernel.GetRequiredService<IChatCompletionService>();
            var chatHistory = new ChatHistory();

            Console.WriteLine("Your prompt: " + request.Messages);
            var prompt = kernel.InvokePromptStreamingAsync(request.Messages);
           

            chatHistory.Add(new ChatMessageContent(AuthorRole.User, request.Messages));


            // Stream the AI response and add to chat history
            Console.WriteLine("AI Response:");
            var response = "";
            await foreach (var item in aiChatService.GetStreamingChatMessageContentsAsync(chatHistory))
            {
                Console.Write(item.Content);
                response += item.Content;
            }
            chatHistory.Add(new ChatMessageContent(AuthorRole.Assistant, response));

            Chat chat = new Chat { Conversation = response };
            return chat;
        }
    }
}
