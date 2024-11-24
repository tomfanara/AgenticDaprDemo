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
                        modelId: "orca-mini",
                        endpoint: new Uri("http://localhost:11434"))
                    .Build();

var aiChatService = kernel.GetRequiredService<IChatCompletionService>();
var chatHistory = new ChatHistory();

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"This simple agentic network will send reports based on questions from human business analyst");
Console.WriteLine("We are using the Phi3.5, Llama3.1 and Orca-Mini LLM models");
Console.WriteLine("The micro agents are Khloe (Accounting supervisor), Carlos (Sales manager) and Jenny (Inventory manager");
Console.WriteLine("");
Console.ResetColor();

var question = "Hi AI, just checking in are you ok?";

// Get user prompt and add to chat history
Console.ForegroundColor = ConsoleColor.Blue;
Console.WriteLine("Your prompt:");
Console.ResetColor();
Console.WriteLine("");
foreach (char c in question)
{
    Console.Write(c);
    Thread.Sleep(25); // Delay in milliseconds
}
Console.WriteLine("");

var prompt = kernel.InvokePromptAsync(question);
//var userPrompt = Console.ReadLine();
chatHistory.Add(new ChatMessageContent(AuthorRole.User, question));


var chatResponse = "";
await foreach (var item in
    aiChatService.GetStreamingChatMessageContentsAsync(chatHistory))
{
    //Console.Write(item.Content);
    chatResponse += item.Content;
}
chatHistory.Add(new ChatMessageContent(AuthorRole.Assistant, chatResponse));

Console.WriteLine("");
Console.ForegroundColor = ConsoleColor.Blue;
Console.WriteLine("AI:");
Console.ResetColor();
Console.WriteLine("");
foreach (char c in chatResponse)
{
    Console.Write(c);
    Thread.Sleep(25); // Delay in milliseconds
}


Console.WriteLine();

var message = new Message {Messages = "I'm good AI. I'm conducting a marketing research project and need to summarize a list of new employees, inventory and sales in accounting. Could you save on my computer." };

Console.WriteLine("");
Console.ForegroundColor = ConsoleColor.Blue;
Console.WriteLine("Your prompt:");
Console.ResetColor();
Console.WriteLine("");
foreach (char c in message.Messages)
{
    Console.Write(c);
    Thread.Sleep(25); // Delay in milliseconds
}

Console.WriteLine("");
Console.WriteLine("");
Console.ForegroundColor = ConsoleColor.Blue;
Console.WriteLine("AI:");
Console.ResetColor();
Console.WriteLine("");

//add delay
Thread.Sleep(2000);

var informingResponse = new Message { Messages = "Hi, I'll have to ask Khloe, Jenny and Carlos for some help on this. Please hold on..." };

foreach (char c in informingResponse.Messages)
{
    Console.Write(c);
    Thread.Sleep(25); // Delay in milliseconds
}

Console.WriteLine("");
Console.WriteLine("");

using (HttpClient client = new HttpClient())
{
    client.Timeout = TimeSpan.FromSeconds(240);
    HttpResponseMessage response = await client.PostAsJsonAsync<Message>("http://localhost:5006/converse", message);
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
            await Task.Delay(25);
        }        
    }
    else
    {
        Console.WriteLine($"Error: {response.StatusCode}");
    }  
}

Console.WriteLine("");
Console.ReadLine();


