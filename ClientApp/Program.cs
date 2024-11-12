﻿#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0003
#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0011
#pragma warning disable SKEXP0050
#pragma warning disable SKEXP0052
#pragma warning disable SKEXP0070

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

// Create a kernel with OpenAI chat completion
// Warning due to the experimental state of some Semantic Kernel SDK features.
#pragma warning disable SKEXP0070
Kernel kernel = Kernel.CreateBuilder()
                    .AddOllamaChatCompletion(
                        modelId: "phi3.5",
                        endpoint: new Uri("http://localhost:11434"))
                    .Build();

var aiChatService = kernel.GetRequiredService<IChatCompletionService>();
var chatHistory = new ChatHistory();


var question = "Hi Phi how are you today?";


//while (true)
//{
    // Get user prompt and add to chat history
    Console.WriteLine("Your prompt:" + question);
    var prompt = kernel.InvokePromptStreamingAsync(question);
    //var userPrompt = Console.ReadLine();
    chatHistory.Add(new ChatMessageContent(AuthorRole.User, question));

    // Stream the AI response and add to chat history
    Console.WriteLine("AI Response:");
    var response = "";
    await foreach (var item in
        aiChatService.GetStreamingChatMessageContentsAsync(chatHistory))
    {
        Console.Write(item.Content);
        response += item.Content;
    }
    chatHistory.Add(new ChatMessageContent(AuthorRole.Assistant, response));
    Console.WriteLine();
//}