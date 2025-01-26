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

public class QueryRewriteActivity : WorkflowActivity<string, string[]>
{
    public async override Task<string[]> RunAsync(WorkflowActivityContext context, string input)
    {
        var question = input;

        var builder = Kernel.CreateBuilder()
                              .AddOllamaChatCompletion(
                               modelId: "llama3",
                               endpoint: new Uri("http://localhost:11434"));
        builder.AddLocalTextEmbeddingGeneration();
        Kernel kernel = builder.Build();

        string fullMessage = "";

        // Read the content of the file
        string fileContent = input; ;
        
        // Split the text into lines
        var lines = new List<string> { fileContent };

        // Define the maximum number of tokens per chunk
        int maxTokensPerChunk = 300;

        // Split the text into chunks
        var chunks = TextChunker.SplitPlainTextParagraphs(lines, maxTokensPerChunk);

        // separator
        //Console.WriteLine("Chunked output from a text document");

        // Output the chunks
        //foreach (var chunk in chunks)
        //{
        //    Console.WriteLine(chunk);
        //}

        // separator
        //Console.WriteLine("");
        //Console.WriteLine("==============");
        //Console.WriteLine("");

        // get the embeddings generator service
        var embeddingGeneratorChunked = kernel.Services.GetRequiredService<ITextEmbeddingGenerationService>();
        var memoryChunked = new SemanticTextMemory(new VolatileMemoryStore(), embeddingGeneratorChunked);

        const string MemoryCollectionNameChunked = "originalPrompt";

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

        var promptChunked = @"You are a helpful assistant that generates search queries "
                    + "based on a single input query. "
                    + "Perform query decomposition and break it down into distinct sub questions that you need to "
                    + "answer in order to answer the original question "
                    + "If there are acronyms and words you are not familiar with, do not try to rephrase them. "
                    + "Return sub questions in CSV content. {{Recall}}";

        var arguments = new KernelArguments(settings)
        {
            { "input", fileContent },
            { "collection", MemoryCollectionNameChunked }
        };

        //Console.WriteLine($"Phi-3 response (using semantic memory and document chunking).");
     
        //var response = kernel.InvokePromptStreamingAsync(promptChunked, arguments);
        await foreach (var result in kernel.InvokePromptStreamingAsync(promptChunked, arguments))
        {         
                //Console.Write(result);
                fullMessage += result.ToString();           
        }

        Console.Write("full message " + fullMessage);

        string[] wordsToSearch = { "employees", "inventory", "sales" };

        var foundWords = wordsToSearch.Where(word => fullMessage.Contains(word)).ToList();

        //string words = "";

        List<string> words = new List<string>();

        if (foundWords.Any())
        {
            Console.WriteLine("Found words:");
            foreach (var word in foundWords)
            {
                words.Add(word);
                Console.WriteLine(word);
            }

        }
        else
        {
            Console.WriteLine("No words found.");
        }

        string[] wordArray = words.ToArray();

        return wordArray;

    }
}