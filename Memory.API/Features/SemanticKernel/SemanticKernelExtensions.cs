using Memory.API.Features.Database;
using Memory.API.Features.SemanticKernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.AI.Ollama;
using Microsoft.KernelMemory.AI;
using Microsoft.KernelMemory.Diagnostics;
using Microsoft.SemanticKernel;

namespace Memory.API.SemanticKernel;

public static class SemanticKernelExtensions
{
    public static async Task AddSemanticKernelAsync(this WebApplicationBuilder builder)
    {
        var logLevel = LogLevel.Warning;
        SensitiveDataLogger.Enabled = false;

        var config = new OllamaConfig
        {
            Endpoint = "http://localhost:11434",
            TextModel = new OllamaModelConfig("phi3", 131072),
            EmbeddingModel = new OllamaModelConfig("phi3", 2048)
        };

        var memory = new KernelMemoryBuilder()
            .WithOllamaTextGeneration(config, new CL100KTokenizer())
            .WithOllamaTextEmbeddingGeneration(config, new CL100KTokenizer())
            .Configure(builder => builder
                .Services
                .AddLogging(l => l
                    .SetMinimumLevel(logLevel)
                    .AddSimpleConsole(c => c.SingleLine = true)))
            .Build();


#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        var kernelBuilder = Kernel.CreateBuilder()
                            .AddOllamaChatCompletion(
                                modelId: "orca-mini",
                                endpoint: new Uri("http://localhost:11434"));
#pragma warning restore SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        Kernel kernel = kernelBuilder.Build();

        var plugin = new MemoryPlugin(memory, "kernelMemory");
        kernel.ImportPluginFromObject(plugin, "memory");

        builder.Services.AddSingleton(kernelBuilder);
        builder.Services.AddSingleton(memory);
        builder.Services.AddTransient<ISemanticKernelService, SemanticKernelService>();

    }
}
