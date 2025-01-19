using Dapr.Workflow;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;

namespace Workflow.API.Features.Activities
{
    public class ReplyToChatHubAcitivity : WorkflowActivity<string, bool>
    {
        public async override Task<bool> RunAsync(WorkflowActivityContext context, string input)
        {
            try
            {
                HubConnection connection;
                connection = new HubConnectionBuilder()
                                .WithUrl("http://localhost:5269/hub/chat")
                                .Build();
                connection.Closed += async (error) =>
                {
                    await Task.Delay(new Random().Next(0, 5) * 1000);
                    await connection.StartAsync();
                };
                try
                { 
                    await connection.StartAsync();
                    Console.WriteLine("connection started");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("connection failed: " + ex.Message);
                }
                Console.WriteLine("SENDING MESSAGE BACK TO UI: " + input);
                await connection.InvokeAsync("SendMessageToClient", input);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR CONNECTING TO CHAT HUB:" +ex.Message);
                return false;
            }
           
        }
    }
}
