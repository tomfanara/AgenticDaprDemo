#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0003
#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0011
#pragma warning disable SKEXP0050
#pragma warning disable SKEXP0052
#pragma warning disable SKEXP0070

using ClientApp.Models.Request;
using Dapr.Client;
using Google.Api;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using OpenAI.Assistants;
using System.Net.Http.Json;
using System.Text.Json;
using System.Net.Http;
using static System.Net.Mime.MediaTypeNames;

var daprClient = new DaprClientBuilder().Build();

// Create a kernel with OpenAI chat completion
// Warning due to the experimental state of some Semantic Kernel SDK features.
#pragma warning disable SKEXP0070
Kernel kernel = Kernel.CreateBuilder()
                    .AddOllamaChatCompletion(
                        modelId: "llama3.1",
                        endpoint: new Uri("http://localhost:11434"))
                    .Build();

var aiChatService = kernel.GetRequiredService<IChatCompletionService>();
var chatHistory = new ChatHistory();

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"This simple agentic network will email reports based on questions from Tracy a human business analyst");
Console.WriteLine("We are using the Phi3.5 LLM model");
Console.WriteLine("The micro agents are Khloe (Accounting supervisor), Carlos (Sales manager) and Jenny (Inventory manager");
Console.WriteLine("");
Console.ResetColor();

var question = "Hi Phi3.5 just checking in, are you ok?";

// Get user prompt and add to chat history
Console.ForegroundColor = ConsoleColor.Blue;
Console.WriteLine("Your prompt:");
Console.ResetColor();
Console.WriteLine("");
foreach (char c in question)
{
    Console.Write(c);
    Thread.Sleep(50); // Delay in milliseconds
}
Console.WriteLine("");

var prompt = kernel.InvokePromptStreamingAsync(question);
    //var userPrompt = Console.ReadLine();
    chatHistory.Add(new ChatMessageContent(AuthorRole.User, question));

// Stream the AI response and add to chat history
Console.WriteLine("");
Console.ForegroundColor = ConsoleColor.Blue;
Console.WriteLine("AI Response:");
Console.ResetColor();
Console.WriteLine("");

var chatResponse = "";
    await foreach (var item in
        aiChatService.GetStreamingChatMessageContentsAsync(chatHistory))
    {
        Console.Write(item.Content);
        chatResponse += item.Content;
    }
    chatHistory.Add(new ChatMessageContent(AuthorRole.Assistant, chatResponse));

Console.WriteLine();

var message = new Message {Messages = "Summarize getting new employees from accounting" };

Console.WriteLine("");
Console.ForegroundColor = ConsoleColor.Blue;
Console.WriteLine("Your prompt:");
Console.ResetColor();
Console.WriteLine("");
foreach (char c in message.Messages)
{
    Console.Write(c);
    Thread.Sleep(50); // Delay in milliseconds
}

Console.WriteLine("");
Console.WriteLine("");
Console.ForegroundColor = ConsoleColor.Blue;
Console.WriteLine("Khloe's Response:");
Console.ResetColor();
Console.WriteLine("");

using (HttpClient client = new HttpClient())
{
    HttpResponseMessage response = await client.PostAsJsonAsync<Message>("http://localhost:5167/converse", message);
    response.EnsureSuccessStatusCode();

    string? line;

    if (response.IsSuccessStatusCode)
    {
        string responseBody = await response.Content.ReadAsStringAsync();
        JsonDocument jsonDocument = JsonDocument.Parse(responseBody);
        JsonElement root = jsonDocument.RootElement;

        string value = root.GetProperty("conversation").GetString();

        foreach (char c in value)
        {
            Console.Write(c);
            await Task.Delay(50);
        }
        
    }
    else
    {
        Console.WriteLine($"Error: {response.StatusCode}");
    }  
}

Console.WriteLine();





