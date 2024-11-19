using Dapr.Client;
using Dapr.Workflow;
using static Workflow.API.Models.TaskChainingModels;

namespace Workflow.API.Features.Activities
{
    public class FanOutFanInAccountingActivity : WorkflowActivity<string, string>
    {
        private readonly DaprClient daprClient;

        public FanOutFanInAccountingActivity(DaprClient daprClient)
        {
            this.daprClient = new DaprClientBuilder().Build();
        }

        public override async Task<string> RunAsync(WorkflowActivityContext context, string steps)
        {
           

            return "hello";
        }
    }
}
