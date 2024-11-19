
namespace Workflow.API.Features.Microagent.Handlers;

using Dapr.Client;
using Dapr.Workflow;
using MediatR;
using Microsoft.Extensions.Hosting;
using Workflow.API.Features.Activities;
using Workflow.API.Features.Workflows;
using Workflow.API.Models;
using static Workflow.API.Models.TaskChainingModels;

    public class InitializeWorkflowCommand : IRequest<string>
    {
        public string? test
        {
            get; set;
        }
    }

    public class InitializeWorkflowCommandHandler(DaprWorkflowClient daprWorkflowClient) : IRequestHandler<InitializeWorkflowCommand, string>
    {
        
        private readonly DaprWorkflowClient daprWorkflowClient = daprWorkflowClient ?? throw new ArgumentNullException(nameof(daprWorkflowClient));
        

        public async Task<string> Handle(InitializeWorkflowCommand request, CancellationToken cancellationToken)
        {           
            // create fake Workflow Task Steps by hydrating the Steps model
            Console.WriteLine("intializing workflow and starting workflow");
       
            var workflowId = $"{Guid.NewGuid().ToString()[..8]}";

            await this.daprWorkflowClient.ScheduleNewWorkflowAsync(           
            name: nameof(TaskChainingWorkflow),
            input: "test",
            instanceId: workflowId);

            WorkflowState state = await daprWorkflowClient.WaitForWorkflowStartAsync(
                                    instanceId: workflowId);

            Console.WriteLine($"{nameof(TaskChainingWorkflow)} (ID = {workflowId}) started successfully");

            return "completed";
        }
    }
