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
using System.Threading.Tasks;


public class QueryRewriteActivity : WorkflowActivity<string, string>
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

        string originalPrompt = "I'm good AI. I'm conducting a marketing research project and need to summarize a list of new employees, inventory and sales in accounting. Could you save on my computer.";
        string system_decompose = @"You are a helpful assistant the generates that generates search queries based on a single "
            + "based on a single input query "
            + "perform query decomposition and break it down into distinct sub questions that you need to "
            + "answer in order to answer the original question "
            + "If there are acronyms and words you are not familiar with, do not try to rephrase them. "
            + "Return sub questions in an array list";

        var argumentsRAG = new KernelArguments(settings)
        {
            { "input", system_decompose },
            { "collection", originalPrompt }
        };

        Console.WriteLine($"Llama3.1 response (using RAG semantic memory and document chunking).");

        var response = kernel.InvokePromptStreamingAsync(originalPrompt, argumentsRAG);
        string fullMessage = "";
        await foreach (var result in response)
        {
            
            fullMessage += result.ToString();
            
        }

        Console.Write(fullMessage);

        return fullMessage;
    }
}
