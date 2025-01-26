using Dapr.Workflow;
using DurableTask.Core.Exceptions;
using Workflow.API.Features.Activities;
using Workflow.API.Models;
using static Workflow.API.Models.TaskChainingModels;

namespace Workflow.API.Features.Workflows;
   
    public class FanOutFanInWorkflow : Workflow<string, string>
    {
        
        public override async Task<string> RunAsync(WorkflowContext context, string prompt)
        {

        // Expotential backoff retry policy that survives long outages
            var retryOptions = new WorkflowTaskOptions
            {
                RetryPolicy = new WorkflowRetryPolicy(
                firstRetryInterval: TimeSpan.FromMilliseconds(10000),
                backoffCoefficient: 2.0,
                maxRetryInterval: TimeSpan.FromHours(1),
                maxNumberOfAttempts: 3),
            };
        

            var tasks = new List<Task<string>>();

            try {

                var prompts = await context.CallActivityAsync<string[]>("QueryRewriteActivity", prompt, retryOptions);

                if (prompts.Length > 0)
                {

                    List<string> domainList = prompts.ToList();

                foreach (var word in domainList)
                {

                    if (word == "employees")
                    {
                        tasks.Add(context.CallActivityAsync<string>("AccountingActivity", "get current employees", retryOptions));
                        continue;
                    }

                    if (word == "inventory")
                    {
                        tasks.Add(context.CallActivityAsync<string>("InventoryActivity", "get current inventory", retryOptions));
                        continue;
                    }
                    if (word == "sales")
                    {
                        tasks.Add(context.CallActivityAsync<string>("SalesActivity", "get current sales", retryOptions));
                        continue;
                    }
                }
                    
                    var messages = await Task.WhenAll(tasks);

                    var reWrite = await context.CallActivityAsync<string>("ResultsRewriteActivity", messages, retryOptions);
                    var chatback = await context.CallActivityAsync<bool>("ReplyToChatHubAcitivity", reWrite, retryOptions);

                    return reWrite;
                }
                else 
                {
                    string clarify = "Hello, our agents respond better to messages that include employees, inventory or sales";
                    var chatback = await context.CallActivityAsync<bool>("ReplyToChatHubAcitivity", clarify, retryOptions);
                    return clarify; 
                }
            }
            catch (TaskFailedException) // Task failures are surfaced as TaskFailedException
            {
                // Retries expired - apply custom compensation logic
                await context.CallActivityAsync<long[]>("MyCompensation", options: retryOptions);
                throw;
            }

           
    }
    }




