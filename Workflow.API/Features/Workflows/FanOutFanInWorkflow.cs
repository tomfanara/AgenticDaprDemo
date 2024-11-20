using Dapr.Workflow;
using Workflow.API.Features.Activities;
using static Workflow.API.Models.TaskChainingModels;

namespace Workflow.API.Features.Workflows;

    
    public class FanOutFanInWorkflow : Workflow<string, string[]>
    {
        public override async Task<string[]> RunAsync(WorkflowContext context, string input)
        {
            var tasks = new List<Task<string>>();
            
            tasks.Add(context.CallActivityAsync<string>(
                nameof(AccountingActivity)));
            tasks.Add(context.CallActivityAsync<string>(
                nameof(InventoryActivity)));
            tasks.Add(context.CallActivityAsync<string>(
                nameof(SalesActivity)));
       
            var messages = await Task.WhenAll(tasks);

            return messages;
        }
    }




