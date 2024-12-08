using Dapr.Actors.Runtime;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Sales.API.Models.Response;

namespace Sales.API.Features.Microagent.Actors
{
    public class SalesActor : Actor, ISales, IRemindable
    {
        private readonly ISalesService salesService;

        public SalesActor(ActorHost host, SalesService salesService) : base(host)
        {
            this.salesService = salesService;
        }

        public Chat GetSales(string prompt)
        {
            return this.salesService.GetSales(prompt);
        }

        public Task<string[]> ListAgentsAsync()
        {
            throw new NotImplementedException();
        }

        public Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
        {
            throw new NotImplementedException();
        }

        public Task SaveChatHistoryAsync(Chat chatHistory)
        {
            throw new NotImplementedException();
        }

        public Task TriggerAlarmForAllAgents()
        {
            throw new NotImplementedException();
        }
    }
}
