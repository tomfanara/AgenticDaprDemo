namespace Workflow.API.Features.Activities;

using Dapr.Workflow;
using DurableTask.Core;
using DurableTask.Core.Exceptions;
using Microsoft.DurableTask.Protobuf;
using System;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using static global::Workflow.API.Models.TaskChainingModels;

public class TaskChainingWorkflow : Workflow<string, string>
{
    public override async Task<string> RunAsync(WorkflowContext context, string prompt)
    {
        // Expotential backoff retry policy that survives long outages
        var retryOptions = new WorkflowTaskOptions
        {
            RetryPolicy = new WorkflowRetryPolicy(
                firstRetryInterval: TimeSpan.FromMinutes(1),
                backoffCoefficient: 2.0,
                maxRetryInterval: TimeSpan.FromHours(1),
                maxNumberOfAttempts: 2),
        };

        try
        {

            var result1 = string.Empty;
            var result2 = string.Empty;
            var result3 = string.Empty;

            var prompts = await context.CallActivityAsync<string[]>("QueryRewriteActivity", prompt, retryOptions);
            int cnt = 0;

            if (prompts.Length > 0)
            {

                List<string> domainList = prompts.ToList();

                foreach (var domain in domainList)
                {
                    if (cnt == prompts.Length)
                    {
                        break;
                    }

                    if (domain == "employees")
                    {
                        result1 = await context.CallActivityAsync<string>("AccountingActivity", "get current employees", retryOptions);
                        
                        continue;
                    }

                    if (domain == "inventory")
                    {
                        result2 = await context.CallActivityAsync<string>("InventoryActivity", "get current inventory", retryOptions);
                        
                        continue;

                    }
                    if (domain == "sales")
                    {
                        result3 = await context.CallActivityAsync<string>("SalesActivity", "get current sales", retryOptions);
                        
                        continue;
                    }

                    cnt++;

                }

                string[] messages = { result1, result2, result3 };

                var result4 = await context.CallActivityAsync<string>("ResultsRewriteActivity", messages, retryOptions);
                var chatback = await context.CallActivityAsync<bool>("ReplyToChatHubAcitivity", result4, retryOptions);
                return string.Join(",\r\n\n", result4);

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
