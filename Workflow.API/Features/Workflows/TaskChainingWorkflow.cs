namespace Workflow.API.Features.Activities;

using Dapr.Workflow;
using DurableTask.Core.Exceptions;
using System;
using static global::Workflow.API.Models.TaskChainingModels;

public class TaskChainingWorkflow : Workflow<Steps, string>
{
    public override async Task<string> RunAsync(WorkflowContext context, Steps steps)
    {
        var retryOptions = new WorkflowTaskOptions
        {
            // put in component later
            RetryPolicy = new WorkflowRetryPolicy(
            firstRetryInterval: TimeSpan.FromMinutes(1),
            backoffCoefficient: 2.0,
            maxRetryInterval: TimeSpan.FromHours(1),
            maxNumberOfAttempts: 10),
        };

        try
        {
            var result = await context.CallActivityAsync<bool>(nameof(TaskChainingActivity), steps);

            // ****wait for business logic endpoint to return or time out after 3 seconds
            var response = await context.WaitForExternalEventAsync<Result>(
              eventName: "ManagerApproval",
              timeout: TimeSpan.FromSeconds(30));
            if (response.successful)
            {
                // assume the external business logic returned true end the workflow here and report results
                var results = string.Join(", ", response.message);
            }

            if (result)
            {
                return "Completed";
            }
        }
        catch (OperationCanceledException) // Task failures are surfaced as TaskFailedException
        {
            // Retries expired - apply custom compensation logic
            Console.WriteLine("cancel step");
            throw;
        }

        // dapr durable looping structure
        steps.currentIndex += 1;
        context.ContinueAsNew(steps, true);
        return "still running...";
    }
}
