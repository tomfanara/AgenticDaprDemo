﻿namespace Accounting.API.Features.Microagent.Handlers
{
    using Accounting.API.Models.Request;
    using Accounting.API.Models.Response;
    using MediatR;
    using Microsoft.SemanticKernel;
    using Microsoft.SemanticKernel.ChatCompletion;
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

            var message = new Message { Messages = "Hi Jenny how are you today?" };
            SendMessage(message);

            Chat chat = new Chat { Conversation = response };
            return chat;
        }

        private async void SendMessage(Message message)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response1 = await client.PostAsJsonAsync<Message>("http://localhost:5279/converse", message);
                response1.EnsureSuccessStatusCode();


                string? line;
                using (Stream stream = await response1.Content.ReadAsStreamAsync())
                using (StreamReader reader = new StreamReader(stream))
                {

                    while ((line = await reader.ReadLineAsync()) != null)
                    {

                        foreach (char c in line)
                        {
                            Console.Write(c);
                            await Task.Delay(50);
                        }
                    }
                }


            }
        }
    }
}
