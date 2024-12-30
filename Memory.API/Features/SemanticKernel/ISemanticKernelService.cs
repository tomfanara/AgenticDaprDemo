using Microsoft.SemanticKernel.ChatCompletion;

namespace Memory.API.Features.SemanticKernel;

public interface ISemanticKernelService
{
    Task ImportText(string text);
    Task<string> Query(string query);
}
