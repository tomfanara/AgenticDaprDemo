// Copyright (c) Alegeus Technologies, LLC. All rights reserved.

namespace Workflow.API.Features.Activities;
#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0003
#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0011
#pragma warning disable SKEXP0050
#pragma warning disable SKEXP0052
#pragma warning disable SKEXP0070

using Dapr.Workflow;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Memory;
using Microsoft.SemanticKernel.Text;
using System.Threading.Tasks;


public class ReRankingActivity : WorkflowActivity<string, string>
{
    public async override Task<string> RunAsync(WorkflowActivityContext context, string input)
    {
        var builder = Kernel.CreateBuilder()
                     .AddOllamaChatCompletion(
                      modelId: "llama3.1",
                      endpoint: new Uri("http://localhost:11434"));
        builder.AddLocalTextEmbeddingGeneration();
        Kernel kernel = builder.Build();

        OpenAIPromptExecutionSettings settings = new()
        {
            ToolCallBehavior = null,
            MaxTokens = 30,
            Temperature = 0,
        };

        // Read the content of the file
        //string fileContent = File.ReadAllText(filePath);

        // Split the text into lines
        var lines = new List<string> { input };

        // Define the maximum number of tokens per chunk
        int maxTokensPerChunk = 60;

        // Split the text into chunks
        var chunks = TextChunker.SplitPlainTextParagraphs(lines, maxTokensPerChunk);

        // separator
        Console.WriteLine("Chunked input from aggregated responses");

        // Output the chunks
        foreach (var chunk in chunks)
        {
            Console.WriteLine(chunk);
        }

        // separator
        Console.WriteLine("");
        Console.WriteLine("==============");
        Console.WriteLine("");

        var embeddingGeneratorChunked = kernel.Services.GetRequiredService<ITextEmbeddingGenerationService>();
        var memoryChunked = new SemanticTextMemory(new VolatileMemoryStore(), embeddingGeneratorChunked);

        const string MemoryCollectionNameChunked = "aggregatedResponses";

        int i = 0;
        foreach (var chunk in chunks)
        {
            await memoryChunked.SaveInformationAsync(MemoryCollectionNameChunked, id: "info" + i, text: chunk);
            i++;
        }

        TextMemoryPlugin memoryPluginChunked = new(memoryChunked);

        // Import the text memory plugin into the Kernel.
        kernel.ImportPluginFromObject(memoryPluginChunked);


        var promptChunked = @"
        Question: I'm conducting a marketing research project and need to summarize a list of new employees, inventory and sales in accounting.{{Recall}}";

        var argumentsRAG = new KernelArguments(settings)
        {
            { "input", input },
            { "collection", MemoryCollectionNameChunked }
        };

        Console.WriteLine($"Llama3.1 response (using RAG semantic memory and document chunking).");

        var response = kernel.InvokePromptStreamingAsync(promptChunked, argumentsRAG);
        string fullMessage = "";
        await foreach (var result in response)
        {
            Console.Write(result);
            fullMessage += result.ToString();
        }
                   
        return await Task.FromResult(fullMessage);
    }
}
