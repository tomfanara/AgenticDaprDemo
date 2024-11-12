namespace Accounting.API.Features.Microagent.Handlers
{
    using Accounting.API.Models.Response;
    using MediatR;
    using Microsoft.SemanticKernel;
    using Microsoft.SemanticKernel.ChatCompletion;
    using System.Text.Json.Nodes;
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

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

            //JObject jsonObject = JObject.Parse(request.ToString());

            //// Get field value
            //string messages = (string)jsonObject["Messages"];
            //Console.WriteLine(messages);  // Output: John


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
