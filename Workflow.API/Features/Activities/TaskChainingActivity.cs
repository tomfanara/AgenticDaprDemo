// Copyright (c) Alegeus Technologies, LLC. All rights reserved.

namespace Workflow.API.Features.Activities;

using Dapr.Client;
using Dapr.Workflow;
using System.Threading.Tasks;
using static global::Workflow.API.Models.TaskChainingModels;


public class TaskChainingActivity : WorkflowActivity<Steps, bool>
{
    private readonly DaprClient daprClient;

    public TaskChainingActivity(DaprClient daprClient)
    {
        this.daprClient = new DaprClientBuilder().Build();
    }

    public override async Task<bool> RunAsync(WorkflowActivityContext context, Steps steps)
    {
        try
        {
            var httpClient = DaprClient.CreateInvokeHttpClient();
            var message = context.InstanceId.ToString();
            var response = await httpClient.PostAsJsonAsync<string>("http://test/" + steps.step[steps.currentIndex].TaskName + "?message=" + message, message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"error:{ex.Message}{ex.StackTrace}{ex.InnerException}");
            return true;
        }

        return await Task.FromResult(steps.currentIndex == (steps.step.Length - 1));
    }
}
