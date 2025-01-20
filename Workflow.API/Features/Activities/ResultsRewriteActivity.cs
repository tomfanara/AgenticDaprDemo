// Copyright (c) Alegeus Technologies, LLC. All rights reserved.

namespace Workflow.API.Features.Activities;

#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0003
#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0011
#pragma warning disable SKEXP0050
#pragma warning disable SKEXP0052
#pragma warning disable SKEXP0070

using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Memory;
using Microsoft.SemanticKernel.Text;
using Dapr.Workflow;
using System.Reactive;
using System.Threading.Tasks;
using static Workflow.API.Models.TaskChainingModels;


public class ResultsRewriteActivity : WorkflowActivity<string[], string>
{
    public async override Task<string> RunAsync(WorkflowActivityContext context, string[] messages)
    {
        string result = "";
        foreach (string message in messages)
        {
            result += message + " ";
        }


        var question = "I'm conducting a marketing research project and need to summarize a list of new employees, inventory and sales.";

        var builder = Kernel.CreateBuilder()
                              .AddOllamaChatCompletion(
                               modelId: "llama3",
                               endpoint: new Uri("http://localhost:11434"));
        builder.AddLocalTextEmbeddingGeneration();
        Kernel kernel = builder.Build();
         
        Console.WriteLine($"Phi-3 response (no memory).");

        string fullMessage = "";

        //var response = kernel.InvokePromptStreamingAsync(question);
        //await foreach (var result in response)
        //{
        //    //Console.Write(result);
        //    fullMessage += result.ToString();
        //}

        // separator
        //Console.WriteLine("");
        //Console.WriteLine("");
        //Console.WriteLine("==============");
        //Console.WriteLine("");

        //string filePath = "aggregate.txt";

        // Read the content of the file
        //string fileContent = File.ReadAllText(input);

        // Split the text into lines
        var lines = new List<string> { result };

        // Define the maximum number of tokens per chunk
        int maxTokensPerChunk = 800;

        // Split the text into chunks
        var chunks = TextChunker.SplitPlainTextParagraphs(lines, maxTokensPerChunk);

        // separator
        //Console.WriteLine("Chunked output from a text document");

        // Output the chunks
        Console.WriteLine("chunks");
        foreach (var chunk in chunks)
        {
            Console.WriteLine(chunk);
        }

        // separator
        //Console.WriteLine("");
        //Console.WriteLine("==============");
        //Console.WriteLine("");

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

        OpenAIPromptExecutionSettings settings = new()
        {
            ToolCallBehavior = null,
            Temperature = 0,
            TopP = 0
        };

        var promptChunked = @"
        Question: I'm conducting a marketing research project and need to summarize a list of new employees, inventory and sales.{{Recall}}.
                  Make sure sales are calculated correctly";

        var arguments = new KernelArguments(settings)
        {
            { "input", question },
            { "collection", MemoryCollectionNameChunked }
        };

        //Console.WriteLine($"Phi-3 response (using semantic memory and document chunking).");

        //var response = kernel.InvokePromptStreamingAsync(promptChunked, arguments);

        
        await foreach (var responses in kernel.InvokePromptStreamingAsync(promptChunked, arguments))
        {
            //Console.Write(result);
            fullMessage += responses.ToString();
        }

        return fullMessage;
    }
}
