
namespace Workflow.API.Features.Microagent.Handlers;

using Dapr.Client;
using Dapr.Workflow;
using MediatR;
using Microsoft.Extensions.Hosting;
using Workflow.API.Features.Activities;
using Workflow.API.Features.Workflows;
using Workflow.API.Models;
using Workflow.API.Models.Response;
using static Google.Rpc.Context.AttributeContext.Types;
using static Workflow.API.Models.TaskChainingModels;

    
    public class TaskChatWorkflowCommandHandler(DaprWorkflowClient daprWorkflowClient) : IRequestHandler<TaskChatHandlerRequest, Chat>
    {
        
        private readonly DaprWorkflowClient daprWorkflowClient = daprWorkflowClient ?? throw new ArgumentNullException(nameof(daprWorkflowClient));
        

        public async Task<Chat> Handle(TaskChatHandlerRequest request, CancellationToken cancellationToken)
        {           
            // create fake Workflow Task Steps by hydrating the Steps model
            Console.WriteLine("intializing workflow and starting workflow");            

            var workflowId = $"{Guid.NewGuid().ToString()[..8]}";

         

            var response = await this.daprWorkflowClient.ScheduleNewWorkflowAsync(           
            name: nameof(FanOutFanInWorkflow),
            input: request.Messages,
            instanceId: workflowId);

            WorkflowState state = await daprWorkflowClient.WaitForWorkflowStartAsync(
                                    instanceId: workflowId);

            Console.WriteLine($"{nameof(FanOutFanInWorkflow)} (ID = {workflowId}) started successfully");

            //Get the status of the workflow
            WorkflowState workflowState;
            while (true)
            {
                workflowState = await daprWorkflowClient.GetWorkflowStateAsync(workflowId, true);
                Console.WriteLine($@"Workflow status: {workflowState.RuntimeStatus}");
                if (workflowState.IsWorkflowCompleted)
                break;

                await Task.Delay(TimeSpan.FromSeconds(1));
            }

        //Display the result from the workflow
        //var result = string.Join(" ", workflowState.ReadOutputAs<string[]>() ?? Array.Empty<string>());
        var result = string.Join(" ", workflowState.ReadOutputAs<string>());
        Chat chat = new Chat{ Conversation = result };
            return chat;
        }
    }
