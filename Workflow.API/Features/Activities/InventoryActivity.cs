using Dapr.Client;
using Dapr.Workflow;
using System.Text.Json;
using Workflow.API.Models.Request;
using Workflow.API.Models.Response;
using static Workflow.API.Models.TaskChainingModels;

namespace Workflow.API.Features.Activities
{
    public class InventoryActivity : WorkflowActivity<string, string>
    {
        private readonly DaprClient daprClient;

        public InventoryActivity(DaprClient daprClient)
        {
            this.daprClient = daprClient;
        }

        public override async Task<string> RunAsync(WorkflowActivityContext context, string messages)
        {

            var message = new Message { Messages = "Hi can you send me the current inventory?" };
            string? value = "";
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.PostAsJsonAsync<Message>("http://localhost:5279/converse", message);
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
