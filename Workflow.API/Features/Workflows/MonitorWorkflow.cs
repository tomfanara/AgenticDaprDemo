namespace Workflow.API.Features.Activities;

using Dapr.Workflow;
using DurableTask.Core.Exceptions;
using System;
using static global::Workflow.API.Models.TaskChainingModels;

public class MonitorWorkflow : Workflow<string, string>
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
            bool isComplete = false;

            if (prompt == "EXIT")
            {
                isComplete = true;
            }

            var response = await context.CallActivityAsync<string>("GroupChatActivity", prompt, retryOptions);

            if (isComplete)
            {
                return "Chat has Exited";
            }

            context.ContinueAsNew(prompt, false);
        }
        catch (TaskFailedException) // Task failures are surfaced as TaskFailedException
        {
            // Retries expired - apply custom compensation logic
            await context.CallActivityAsync<long[]>("MyCompensation", options: retryOptions);
            throw;
        }

        // Ensure a return statement is present for all code paths
        return "Workflow continued";
    }
}
