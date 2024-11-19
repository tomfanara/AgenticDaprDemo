namespace Workflow.API.Features.Activities;

using Dapr.Workflow;
using DurableTask.Core.Exceptions;
using System;
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
                maxNumberOfAttempts: 10),
        };

        try
        {
            var result1 = await context.CallActivityAsync<string>("FanOutFanInAccountingActivity", prompt, retryOptions);
            var result2 = await context.CallActivityAsync<string> ("FanOutFanInInventoryActivity", prompt, retryOptions);
            var result3 = await context.CallActivityAsync <string> ("FanOutFanInSalesActivity", prompt, retryOptions);
            //return string.Join(", ", result1, result2, result3);
            return "hello";
        }
        catch (TaskFailedException) // Task failures are surfaced as TaskFailedException
        {
            // Retries expired - apply custom compensation logic
            await context.CallActivityAsync<long[]>("MyCompensation", options: retryOptions);
            throw;
        }

    }
}
