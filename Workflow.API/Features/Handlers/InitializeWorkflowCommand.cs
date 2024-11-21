
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

            var prompts = new Prompt[3];

            var prompt1 = new Prompt();
            prompt1.Index = 0;
            prompt1.AgentAPIEndpoint = "/converse";
            prompt1.AgentName = "Khloe";
            prompt1.Message = "Hi Klhoe...";

            var prompt2 = new Prompt();
            prompt2.Index = 0;
            prompt2.AgentAPIEndpoint = "/converse";
            prompt2.AgentName = "Jenny";
            prompt2.Message = "Hi Jenny...";

            var prompt3 = new Prompt();
            prompt3.Index = 0;
            prompt3.AgentAPIEndpoint = "/converse";
            prompt3.AgentName = "Carlos";
            prompt3.Message = "Hi Carlos...";

            prompts[0] = prompt1;
            prompts[1] = prompt2;
            prompts[2] = prompt3;

            var promptCollection = new Prompts(0, prompts);

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
