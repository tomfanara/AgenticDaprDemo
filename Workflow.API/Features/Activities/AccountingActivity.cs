using Dapr.Client;
using Dapr.Workflow;
using Google.Protobuf.WellKnownTypes;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks.Dataflow;
using Workflow.API.Models.Request;
using Workflow.API.Models.Response;
using static Google.Rpc.Context.AttributeContext.Types;
using static Workflow.API.Models.TaskChainingModels;

namespace Workflow.API.Features.Activities
{
    public class AccountingActivity : WorkflowActivity<string, string>
    {
        private readonly DaprClient daprClient;

        public AccountingActivity(DaprClient daprClient)
        {
            this.daprClient = daprClient;
        }

        public override async Task<string> RunAsync(WorkflowActivityContext context, string messages)
        {
            var message = new Message { Messages = messages };
            string? value = "";
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.PostAsJsonAsync<Message>("http://localhost:5167/converse", message);
                response.EnsureSuccessStatusCode();               

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    JsonDocument jsonDocument = JsonDocument.Parse(responseBody);
                    JsonElement root = jsonDocument.RootElement;

                    value = root.GetProperty("conversation").GetString();
                    
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode}");
                }
            }

            Console.WriteLine(value);
            Chat chat = new Chat { Conversation = value };
            return await Task.FromResult(chat.Conversation);
        }
    }
}
