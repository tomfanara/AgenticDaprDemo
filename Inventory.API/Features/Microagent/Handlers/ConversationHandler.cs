namespace Inventory.API.Features.Microagent.Handlers;

using Inventory.API.Models.Response;
using MediatR;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json.Nodes;
using System;
using Inventory.API.Models.Request;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Inventory.API.Features.Microagent.Personas;
using Accounting.API.Features.Microagent.Plugins;

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
                      modelId: "llama3.1",
                      endpoint: new Uri("http://localhost:11434"));

        //register plugins
        builder.Plugins.AddFromType<InventoryPlugin>();

        Kernel kernel = builder.Build();

        //check if plugin registered
        //foreach (var pluginName in kernel.Plugins)
        //{
        //    Console.WriteLine(pluginName);
        //}

        //create a persona Khloe
        Persona persona = new Persona
        {
            Name = "Jenny",
            Tone = "shy",
            Style = "terse",
            Traits = new List<string> { "quiet", "helpful", "timid" }
        };

        var settings = new PersonaSettings();
        ApplyPersona(settings, persona);

        string greeting = settings.GenerateResponse(request.Messages);

        Console.WriteLine("");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("Jenny's prompt:");
        Console.ResetColor();
        Console.WriteLine("");
        Console.WriteLine(greeting);

        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        string promptTemplate = request.Messages + ": " + "{{get_inventory}}";
        //string promptTemplate = request.Messages;
        //var result = await kernel.InvokePromptAsync(promptTemplate);

        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage(promptTemplate);

        var result = await kernel.InvokePromptAsync(
        promptTemplate,
        new(new OpenAIPromptExecutionSettings()
        {
            MaxTokens = 50,
            Temperature = 0,
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
            //ChatSystemPrompt = @"{{save_data}}"
        }));

        //OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        //{
        //    Temperature = 0,
        //    MaxTokens = 200,
        //    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
        //    ChatSystemPrompt = @"{get_employees}"
        //};

        //var chatResponse = "";

        //var chatResponse = chatService.GetStreamingChatMessageContentsAsync(chatHistory, openAIPromptExecutionSettings, kernel: kernel);
        //var fullMessage = "";
        //await foreach (var content in chatResponse)
        //{
        //    Console.Write(content.Content);
        //    fullMessage += content.Content;
        //}

        chatHistory.Add(new ChatMessageContent(AuthorRole.Assistant, result.GetValue<string>()));

        Console.WriteLine(result.GetValue<string>());

        chatHistory.Add(new ChatMessageContent(AuthorRole.Assistant, result.GetValue<string>()));

        var message = new Message { Messages = "Hi Carlos, can you send the current iPad Sales?" };
        SendMessage(message);

        Chat chat = new Chat { Conversation = result.GetValue<string>() };
        return chat;
    }

    private async void SendMessage(Message message)
    {
        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response1 = await client.PostAsJsonAsync<Message>("http://localhost:5005/converse", message);
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


