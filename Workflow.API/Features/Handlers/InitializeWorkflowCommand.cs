
namespace Workflow.API.Features.Microagent.Handlers;

using Dapr.Client;
using MediatR;
using Poc.Workflow.TaskChaining.Api.Features.WorkflowTypes.TaskChaining.Application.Workflows;
using Workflow.API.Features.Activities;
using Workflow.API.Models;
using static Workflow.API.Models.TaskChainingModels;

public class InitializeWorkflowCommand : IRequest<string>
    {
        public string? test
        {
            get; set;
        }
    }

    public class InitializeWorkflowCommandHandler : IRequestHandler<InitializeWorkflowCommand, string>

    {
        private readonly DaprClient daprClient;
        private const string DaprWorkflowComponent = "dapr";

        public InitializeWorkflowCommandHandler()
        {
            this.daprClient = new DaprClientBuilder().Build();
        }

        public async Task<string> Handle(InitializeWorkflowCommand request, CancellationToken cancellationToken)
        {
            // create fake Workflow Task Steps by hydrating the Steps model
            Console.WriteLine("intializing workflow type and starting workflow");

            var step = new Step[2];

            var step1 = new Step();
            step1.Index = 0;
            step1.TaskName = "Hello";
            step1.BusinessLogicEndpoint = "/hello";
            step1.CancelLogicEndpoint = "/hellocancel";

            var step2 = new Step();
            step2.Index = 1;
            step2.TaskName = "GoodBye";
            step2.BusinessLogicEndpoint = "/goodbye";
            step2.CancelLogicEndpoint = "goodbyecancel";

            step[0] = step1 as Step;
            step[1] = step2 as Step;

            var steps = new Steps(0, step);

            var workflowId = $"{Guid.NewGuid().ToString()[..8]}";

#pragma warning disable CS0618 // Type or member is obsolete
            await this.daprClient.StartWorkflowAsync(
            workflowComponent: DaprWorkflowComponent,
            workflowName: nameof(TaskChainingWorkflow),
            input: steps,
            instanceId: workflowId,
            cancellationToken: cancellationToken);
#pragma warning restore CS0618 // Type or member is obsolete
            return "completed";
        }
    }
