using Dapr.Client;
using Dapr.Workflow;
using System.Text.Json;
using System.Threading.Tasks.Dataflow;
using Workflow.API.Models.Request;
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

        public override async Task<string> RunAsync(WorkflowActivityContext context, string message)
        {

            //var httpClient = DaprClient.CreateInvokeHttpClient();
            //Message catMessage = new Message { Messages = message };
            //var response = await httpClient.PostAsJsonAsync<string>("http://localhost:5167/converse", message);

            string value = message;
            //if (response.IsSuccessStatusCode)
            //{
            //    string responseBody = await response.Content.ReadAsStringAsync();
            //    JsonDocument jsonDocument = JsonDocument.Parse(responseBody);
            //    JsonElement root = jsonDocument.RootElement;

            //    value = root.GetProperty("conversation").GetString();
              
            //}
            //else
            //{
            //    Console.WriteLine($"Error: {response.StatusCode}");
            //}

            return await Task.FromResult(message);
        }
    }
}
