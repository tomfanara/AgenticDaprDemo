using Dapr.Client;
using Dapr.Workflow;
using static Workflow.API.Models.TaskChainingModels;

namespace Workflow.API.Features.Activities
{
    public class InventoryActivity : WorkflowActivity<string, string>
    {
        private readonly DaprClient daprClient;

        public InventoryActivity(DaprClient daprClient)
        {
            this.daprClient = daprClient;
        }

        public override async Task<string> RunAsync(WorkflowActivityContext context, string steps)
        {


            return "good bye";
        }
    }
}
