using Dapr.Client;
using Dapr.Workflow;
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

        public override async Task<string> RunAsync(WorkflowActivityContext context, string message)
        {

            //var httpClient = DaprClient.CreateInvokeHttpClient();
            //Message catMessage = new Message { Messages = message };
            //var response = await httpClient.PostAsJsonAsync<string>("http://localhost:5167/converse", message);

            string chatResponse = message;
            //if (response.IsSuccessStatusCode)
            //{
            //    string responseBody = await response.Content.ReadAsStringAsync();
            //    JsonDocument jsonDocument = JsonDocument.Parse(responseBody);
            //    JsonElement root = jsonDocument.RootElement;

            //    chatResponse = root.GetProperty("conversation").GetString();

            //}
            //else
            //{
            //    Console.WriteLine($"Error: {response.StatusCode}");
            //}

            Chat chat = new Chat { Conversation = chatResponse };

            return await Task.FromResult(chat.Conversation);
        }
    }
}
