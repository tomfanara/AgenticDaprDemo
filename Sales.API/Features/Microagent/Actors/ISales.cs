using Dapr.Actors;
using Microsoft.SemanticKernel.ChatCompletion;
using Sales.API.Models.Response;

namespace Sales.API.Features.Microagent.Actors
{
    public interface ISales : IActor
    {
        Task<Chat> GetSales(string prompt);
        Task<string> SaveChatHistoryAsync(SalesChatHistoryData chatHistory);
        Task<string[]> ListAgentsAsync();
    }

    public class SalesChatHistoryData
    {
        public ChatHistory ChatHistory { get; set; } = default!;
    }
}
