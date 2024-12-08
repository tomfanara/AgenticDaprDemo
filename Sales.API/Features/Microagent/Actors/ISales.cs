using Dapr.Actors;
using Sales.API.Models.Response;

namespace Sales.API.Features.Microagent.Actors
{
    public interface ISales : IActor
    {
        Chat GetSales(string prompt);
        Task SaveChatHistoryAsync(Chat chatHistory);
        Task<string[]> ListAgentsAsync();
        Task TriggerAlarmForAllAgents();
    }

    public class SalesChatHistoryData
    {
        public string ChatHistory { get; set; } = default!;
    }
}
