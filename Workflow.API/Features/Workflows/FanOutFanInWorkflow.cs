using Dapr.Workflow;
using DurableTask.Core.Exceptions;
using Workflow.API.Features.Activities;
using static Workflow.API.Models.TaskChainingModels;

namespace Workflow.API.Features.Workflows;

    
    public class FanOutFanInWorkflow : Workflow<string, string[]>
    {
        
        public override async Task<string[]> RunAsync(WorkflowContext context, string input)
        {

        // Expotential backoff retry policy that survives long outages
            var retryOptions = new WorkflowTaskOptions
            {
                RetryPolicy = new WorkflowRetryPolicy(
                firstRetryInterval: TimeSpan.FromMinutes(1),
                backoffCoefficient: 2.0,
                maxRetryInterval: TimeSpan.FromHours(1),
                maxNumberOfAttempts: 10),
            };

            var tasks = new List<Task<string>>();

            try { 
            
                tasks.Add(context.CallActivityAsync<string>(
                nameof(AccountingActivity), retryOptions));
                tasks.Add(context.CallActivityAsync<string>(
                nameof(InventoryActivity), retryOptions));
                tasks.Add(context.CallActivityAsync<string>(
                nameof(SalesActivity), retryOptions));
       
                var messages = await Task.WhenAll(tasks);

                return messages;
            }
            catch (TaskFailedException) // Task failures are surfaced as TaskFailedException
            {
                // Retries expired - apply custom compensation logic
                await context.CallActivityAsync<long[]>("MyCompensation", options: retryOptions);
                throw;
            }
        }
    }




