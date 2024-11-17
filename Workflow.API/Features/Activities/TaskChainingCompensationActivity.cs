// Copyright (c) Alegeus Technologies, LLC. All rights reserved.

namespace Workflow.API.Features.Activities;

using Dapr.Workflow;
using System.Reactive;
using System.Threading.Tasks;


public class TaskChainingCompensationActivity : WorkflowActivity<object, object>
{
    public override Task<object> RunAsync(WorkflowActivityContext context, object input)
    {
        throw new NotImplementedException();
    }
}
