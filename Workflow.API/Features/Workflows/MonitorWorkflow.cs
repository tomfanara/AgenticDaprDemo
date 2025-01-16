namespace Workflow.API.Features.Activities;

using Dapr.Workflow;
using DurableTask.Core.Exceptions;
using System;
using static global::Workflow.API.Models.TaskChainingModels;

public class MonitorWorkflow : Workflow<string, string>
{
    string result = "";
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
        string? response;
        try
        {
            //// Pause and wait for a human prompt
            string promptResult = await context.WaitForExternalEventAsync<string>(
                eventName: "PromptMessage",
                timeout: TimeSpan.FromSeconds(180));

            response = await context.CallActivityAsync<string>("GroupChatActivity", promptResult, retryOptions);
            Console.WriteLine("START ACTIVITY ReplyToChatHubAcitivity:");
            var rep= await context.CallActivityAsync<bool>("ReplyToChatHubAcitivity", response, retryOptions);
                if (rep)
                {
                    result = response;
                    Console.WriteLine(response);
                }
            
            

            if (promptResult == "EXIT")
            {
                // The order was rejected, end the workflow here
                Console.WriteLine(promptResult);             
                return "Chat has Exited";
            }
            
        }
        catch (OperationCanceledException) // Task failures are surfaced as TaskFailedException
        {
            // Retries expired - apply custom compensation logic
            await context.CallActivityAsync<string>("CompensationActivity", "Good bye", options: retryOptions);
            return "Chat has timed out";
            //throw;
        }

        // Ensure a return statement is present for all code paths
        context.ContinueAsNew(result, false);
        return response;

    }
}
