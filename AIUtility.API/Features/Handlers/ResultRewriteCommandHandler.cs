
namespace AIUtility.API.Features.Microagent.Handlers;

using AIUtility.API.Models.Response;
using MediatR;

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

public class ResultRewriteCommandHandler : IRequestHandler<ResultRewriteHandlerRequest, Rewrite>
{
    public async Task<Rewrite> Handle(ResultRewriteHandlerRequest request, CancellationToken cancellationToken)
    {
        string result = "";
        if (request.Messages != null)
        {
            foreach (var message in request.Messages)
            {
                result += message + " ";
            }
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

        var lines = new List<string> { result };

        int maxTokensPerChunk = 800;

        var chunks = TextChunker.SplitPlainTextParagraphs(lines, maxTokensPerChunk);

        foreach (var chunk in chunks)
        {
            Console.WriteLine(chunk);
        }

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

        kernel.ImportPluginFromObject(memoryPluginChunked);

        OpenAIPromptExecutionSettings settings = new()
        {
            ToolCallBehavior = null,
            Temperature = 1,
            TopP = .5
        };

        var promptChunked = @"
        Question: I'm conducting a marketing research project and need to summarize a list of new employees, inventory and sales.{{Recall}}";

        var arguments = new KernelArguments(settings)
        {
            { "input", question },
            { "collection", MemoryCollectionNameChunked }
        };

        var response = kernel.InvokePromptStreamingAsync(promptChunked, arguments);

        await foreach (var responses in response)
        {
            fullMessage += responses.ToString();
        }

        Rewrite chat = new Rewrite { Conversation = fullMessage };
        return chat;
    }
}
