﻿namespace Sales.API.Features.Microagent.Handlers;

#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0003
#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0011
#pragma warning disable SKEXP0050
#pragma warning disable SKEXP0052
#pragma warning disable SKEXP0070

using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Memory;
using Microsoft.SemanticKernel.Text;
using Sales.API.Features.Microagent.Personas;
using Sales.API.Models.Response;

public class ConversationHandler()
: IRequestHandler<ConversationHandlerRequest, Chat>
{
    public async Task<Chat> Handle(ConversationHandlerRequest request, CancellationToken cancellationToken)
    {        var question = "summarize current total sales";
        //Console.WriteLine($"This program will answer the following question: {question}");
        //Console.WriteLine("1st approach will be to ask the question directly to the Phi-3 model.");
        //Console.WriteLine("2nd approach will be to add facts to a semantic memory and ask the question again");
        //Console.WriteLine("");

        var builder = Kernel.CreateBuilder()
                              .AddOllamaChatCompletion(
                               modelId: "phi3",
                               endpoint: new Uri("http://localhost:11434"));
        builder.AddLocalTextEmbeddingGeneration();
        Kernel kernel = builder.Build();

        //create a persona Khloe
        Persona persona = new Persona
        {
            Name = "Carlos",
            Tone = "friendly",
            Style = "efficient",
            Traits = new List<string> { "cool", "efficient", "approachable" }
        };

        var personaSettings = new PersonaSettings();
        ApplyPersona(personaSettings, persona);

        string greeting = personaSettings.GenerateResponse(request.Messages);

        Console.WriteLine("");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("Carlos's prompt:");
        Console.ResetColor();
        Console.WriteLine("");
        Console.WriteLine(greeting);

        string filePath = "./data/sales.txt";

        // Read the content of the file
        string fileContent = File.ReadAllText(filePath);

        // Split the text into lines
        var lines = new List<string> { fileContent };

        // Define the maximum number of tokens per chunk
        int maxTokensPerChunk = 60;

        // Split the text into chunks
        var chunks = TextChunker.SplitPlainTextParagraphs(lines, maxTokensPerChunk);

        // separator
        Console.WriteLine("Chunked output from a text document");

        // Output the chunks
        foreach (var chunk in chunks)
        {
            Console.WriteLine(chunk);
        }

        // separator
        Console.WriteLine("");
        Console.WriteLine("==============");
        Console.WriteLine("");

        // get the embeddings generator service
        var embeddingGeneratorChunked = kernel.Services.GetRequiredService<ITextEmbeddingGenerationService>();
        var memoryChunked = new SemanticTextMemory(new VolatileMemoryStore(), embeddingGeneratorChunked);

        const string MemoryCollectionNameChunked = "SalesChunked";

        int i = 0;
        foreach (var chunk in chunks)
        {
            await memoryChunked.SaveInformationAsync(MemoryCollectionNameChunked, id: "info" + i, text: chunk);
            i++;
        }

        TextMemoryPlugin memoryPluginChunked = new(memoryChunked);

        // Import the text memory plugin into the Kernel.
        kernel.ImportPluginFromObject(memoryPluginChunked);

        OpenAIPromptExecutionSettings settings = new()
        {
            ToolCallBehavior = null,
            Temperature = 0,
            TopP = 0,
        };

        var promptChunked = @"
        Question: calculate total revenue per item and give a grand total. 
        Answer the question using the memory content: {{Recall}}";

        var arguments = new KernelArguments(settings)
        {
            { "input", question },
            { "collection", MemoryCollectionNameChunked }
        };

        Console.WriteLine($"llama3 response (using semantic memory and document chunking).");

        string fullMessage = "";
        var response = kernel.InvokePromptStreamingAsync(promptChunked, arguments);
        await foreach (var result in response)
        {
            Console.Write(result);
            fullMessage += result;
        }
        //chatHistory.AddUserMessage(fullMessage);

        //Console.WriteLine($" The end!");

        Console.WriteLine($"");
        Chat chat = new Chat { Conversation = fullMessage };
        return chat;

    }

    private static void ApplyPersona(PersonaSettings settings, Persona persona)
    {
        settings.SetTone(persona.Tone);
        settings.SetStyle(persona.Style);
        settings.SetTraits(persona.Traits);
    }
}