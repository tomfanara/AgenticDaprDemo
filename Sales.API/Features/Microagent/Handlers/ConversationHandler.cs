namespace Sales.API.Features.Microagent.Handlers;


#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0003
#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0011
#pragma warning disable SKEXP0050
#pragma warning disable SKEXP0052

using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
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
    {
        // Create a chat completion service
        #pragma warning disable SKEXP0070
        var builder = Kernel.CreateBuilder()
                      .AddOllamaChatCompletion(
                       modelId: "llama3.1",
                       endpoint: new Uri("http://localhost:11434"));
        builder.AddLocalTextEmbeddingGeneration();
        Kernel kernel = builder.Build();

        //create a persona Khloe
        Persona persona = new Persona
        {
            Name = "Carlos",
            Tone = "very friendly",
            Style = "conversational",
            Traits = new List<string> { "fun", "helpful", "approachable" }
        };

        var settings = new PersonaSettings();
        ApplyPersona(settings, persona);

        string greeting = settings.GenerateResponse(request.Messages);

        Console.WriteLine("");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("Carlos's prompt:");
        Console.ResetColor();
        Console.WriteLine("");
        Console.WriteLine(greeting);


        Console.WriteLine($"Phi-3 response (no memory).");
        var response = kernel.InvokePromptStreamingAsync("What's Tom's favorite sport?");
        await foreach (var result in response)
        {
            Console.Write(result);
        }
           
        string filePath = "ragai.txt";

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

        const string MemoryCollectionNameChunked = "fanFactsChunked";

        int i = 0;
        foreach (var chunk in chunks)
        {
            await memoryChunked.SaveInformationAsync(MemoryCollectionNameChunked, id: "info" + i, text: chunk);
            i++;
        }

        TextMemoryPlugin memoryPluginChunked = new(memoryChunked);

        // Import the text memory plugin into the Kernel.
        kernel.ImportPluginFromObject(memoryPluginChunked);

        OpenAIPromptExecutionSettings kernelSettings = new()
        {
            ToolCallBehavior = null,
            Temperature = 0,
        };

        var promptChunked = @"
            Question: What is Tom's favorite sport?
            Answer the question using the memory content: {{Recall}}";

        var arguments = new KernelArguments(kernelSettings)
        {
        { "input", "What's Tom's favorite sport?" },
        { "collection", MemoryCollectionNameChunked }
        };

        Console.WriteLine($"Phi-3 response (using semantic memory and document chunking).");

        response = kernel.InvokePromptStreamingAsync(promptChunked, arguments);
        var fullMessage = "";
        await foreach (var result in response)
        {
            Console.Write(result);
            fullMessage += result.ToString();
        }

        //var fullMessage = "";
        //await foreach (var content in chatResponse)
        //{
        //    Console.Write(content.Content);
        //    fullMessage += content.Content;
        //}

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
