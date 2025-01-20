using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using Workflow.API.Models.Request;
using Workflow.API.Models.Response;

namespace Workflow.API.Features.Plugins
{
    public class AccountingPlugin
    {
        [KernelFunction("get_employees"), Description("Get employees")]
        [return: Description("A list of employees from accounting.")]
        public async Task<string> GetEmployees()
        {
            var message = new Message { Messages = "Get current inventory" };
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
            return chat.Conversation;
        }
    }
}
