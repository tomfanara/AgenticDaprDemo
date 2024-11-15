namespace Accounting.API.Features.Microagent.Handlers
{
    using Accounting.API.Features.Microagent.Personas;
    using Accounting.API.Features.Microagent.Plugins;
    using Accounting.API.Models.Request;
    using Accounting.API.Models.Response;
    using MediatR;
    using Microsoft.SemanticKernel;
    using Microsoft.SemanticKernel.ChatCompletion;
    using Microsoft.SemanticKernel.Connectors.OpenAI;
    using System;
   
    public class ConversationHandler()
        : IRequestHandler<ConversationHandlerRequest, Chat>
    {
              
        public async Task<Chat> Handle(ConversationHandlerRequest request, CancellationToken cancellationToken)
        {

            // Create a kernel with OpenAI chat completion
            // Warning due to the experimental state of some Semantic Kernel SDK features.
#pragma warning disable SKEXP0070
            var builder = Kernel.CreateBuilder()
                                .AddOllamaChatCompletion(
                                    modelId: "phi3.5",
                                    endpoint: new Uri("http://localhost:11434"));           

            builder.Plugins.AddFromType<NewsPlugin>();
            
            Kernel kernel = builder.Build();            
            
            foreach (var pluginName in kernel.Plugins)
            {
                Console.WriteLine(pluginName);
            }

            //create a persona Khloe
            Persona persona = new Persona
            {
                Name = "Khloe",
                Tone = "friendly",
                Style = "conversational",
                Traits = new List<string> { "empathetic", "helpful", "approachable" }
            };

            var settings = new PersonaSettings();
            ApplyPersona(settings, persona);
                                 
            string greeting = settings.GenerateResponse(request.Messages);

            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Khloe's Response:");
            Console.ResetColor();
            Console.WriteLine("");
            Console.WriteLine(greeting);

            var chatService = kernel.GetRequiredService<IChatCompletionService>();
                      
            string promptTemplate = request.Messages + ": " + "{{execute_query}}";
            //var result = await kernel.InvokePromptAsync(promptTemplate);

            var result = await kernel.InvokePromptAsync(
            promptTemplate,
            new(new OpenAIPromptExecutionSettings()
            {
                MaxTokens = 50,
                Temperature = 1
            }));
            Console.WriteLine(result.GetValue<string>());

            var chatMessages = new ChatHistory();
            chatMessages.AddUserMessage(promptTemplate);
            
            OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
            {

                Temperature = 1.0,
                MaxTokens = 200,
               
            };

            //var completion = chatService.GetStreamingChatMessageContentsAsync(
            //    chatMessages, openAIPromptExecutionSettings,                
            //    kernel);

            ////// Stream the AI response and add to chat history
            ////Console.WriteLine("Khloe's Response:");
            //var fullMessage = "";
            //await foreach (var content in completion)
            //{
            //    Console.Write(content.Content);
            //    fullMessage += content.Content;
            //}

            chatMessages.Add(new ChatMessageContent(AuthorRole.Assistant, result.GetValue<string>()));

            //var message = new Message { Messages = "Hi Jenny how are you today?" };
            //SendMessage(message);

            Chat chat = new Chat { Conversation = result.GetValue<string>() };
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

        private static void ApplyPersona(PersonaSettings settings, Persona persona)
        {
            settings.SetTone(persona.Tone);
            settings.SetStyle(persona.Style);
            settings.SetTraits(persona.Traits);
        }


    }
}
