
namespace Workflow.API.Features.Microagent.Handlers;

using Dapr.Client;
using MediatR;
using static Workflow.API.Models.TaskChainingModels;

public class RaiseEventWorkflowCommand : IRequest<string>
    {
        public string? message
        {
            get; set;
        }
    }

    public class RaiseEventWorkflowCommandHandler : IRequestHandler<RaiseEventWorkflowCommand, string>

    {
        private readonly DaprClient daprClient;
        private const string DaprWorkflowComponent = "dapr";

        public RaiseEventWorkflowCommandHandler()
        {
            this.daprClient = new DaprClientBuilder().Build();
        }

        public async Task<string> Handle(RaiseEventWorkflowCommand request, CancellationToken cancellationToken)
        {
            // raise event from client to let workflow continue
            Result res = new Result(true, "success");

            // can only call in same appId for now and instance Id must match workflow instance Id

            await daprClient.RaiseWorkflowEventAsync(
            instanceId: request.message,
            workflowComponent: DaprWorkflowComponent,
            eventName: "ManagerApproval",
            eventData: res);

            return "completed";
        }
    }

