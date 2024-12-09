using Dapr.Actors.Runtime;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel.ChatCompletion;
using Sales.API.Models.Response;

namespace Sales.API.Features.Microagent.Actors
{
    public class SalesActor : Actor, ISales, IRemindable
    {
        private readonly ISalesService salesService;
        private readonly string chatDataKey = "chat-data";

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

        public async Task<string> SaveChatHistoryAsync(SalesChatHistoryData chatHistory)
        {
            // This set state action can happen along other state changing operations in each actor method and those changes will be maintained
            // in a local cache to be committed as a single transaction to the backing store when the method has completed. As such, there is 
            // no need to (and in fact makes your code less transactional) call `this.StateManager.SaveStateAsync()` as it will be automatically
            // invoked by the actor runtime following the conclusion of this method as part of the internal `OnPostActorMethodAsyncInternal` method.

            // Note also that all saved state must be DataContract serializable.
            await StateManager.SetStateAsync<SalesChatHistoryData>(chatDataKey, chatHistory);

            return "Success";
        }

    }
}
