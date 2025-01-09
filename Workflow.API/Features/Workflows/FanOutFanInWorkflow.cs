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

                tasks.Add(context.CallActivityAsync<string>("AccountingActivity", prompts[1], retryOptions));
                tasks.Add(context.CallActivityAsync<string>("InventoryActivity", prompts[2], retryOptions));
                tasks.Add(context.CallActivityAsync<string>("SalesActivity", prompts[3], retryOptions));

                var messages = await Task.WhenAll(tasks);
                //string result = "";

                //foreach (string message in messages)
                //{ 
                //    result += message + " "; 
                //}

                var reWrite = await context.CallActivityAsync<string>("ResultsRewriteActivity", messages, retryOptions);
                
                return reWrite;
            }
            catch (TaskFailedException) // Task failures are surfaced as TaskFailedException
            {
                // Retries expired - apply custom compensation logic
                await context.CallActivityAsync<long[]>("MyCompensation", options: retryOptions);
                throw;
            }

           
    }
    }




