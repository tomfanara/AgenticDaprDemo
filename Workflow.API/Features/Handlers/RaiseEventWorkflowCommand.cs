
namespace Workflow.API.Features.Microagent.Handlers;

using Dapr.Client;
using Dapr.Workflow;
using MediatR;
using Workflow.API.Models.Response;
using static Workflow.API.Models.TaskChainingModels;

    public class RaiseEventWorkflowCommandHandler : IRequestHandler<RaiseEventHandlerRequest, Chat>

    {
        private readonly DaprClient daprClient;
        private const string DaprWorkflowComponent = "dapr";

        public RaiseEventWorkflowCommandHandler()
        {
            this.daprClient = new DaprClientBuilder().Build();
        }

    [Obsolete]
    public async Task<Chat> Handle(RaiseEventHandlerRequest request, CancellationToken cancellationToken)
        {
            // raise event from client to let workflow continue
            //Result res = new Result(true, "success");

        // can only call in same appId for now and instance Id must match workflow instance Id

#pragma warning disable CA2016 // Forward the 'CancellationToken' parameter to methods
        await daprClient.RaiseWorkflowEventAsync(
            instanceId: "12345678",
            workflowComponent: DaprWorkflowComponent,
            eventName: "PromptMessage",
            eventData: request.Messages);
#pragma warning restore CA2016 // Forward the 'CancellationToken' parameter to methods

        //var result = string.Join(" ", workflowState.ReadOutputAs<string>());
        Chat chat = new Chat { Conversation = "hello"};
        return chat;
    }
}
  

