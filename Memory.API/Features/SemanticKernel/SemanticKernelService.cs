using Memory.API.Features.SemanticKernel;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Memory.API.SemanticKernel;

public class SemanticKernelService(Kernel kernel, IKernelMemory memory) : ISemanticKernelService
{
    public async Task ImportText(string text)
    {
        await memory.ImportTextAsync(text);
    }

    public async Task<string> Query(string query)
    {
        var result = await memory.AskAsync(query);
        return result.Result;
    }
}