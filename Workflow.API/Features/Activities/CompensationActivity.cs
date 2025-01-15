// Copyright (c) Alegeus Technologies, LLC. All rights reserved.

namespace Workflow.API.Features.Activities;

using Dapr.Workflow;
using System.Reactive;
using System.Threading.Tasks;
using Workflow.API.Models;


public class CompensationActivity : WorkflowActivity<string, string>
{
    public override Task<string> RunAsync(WorkflowActivityContext context, string input)
    {

        Console.WriteLine(input);
        return Task.FromResult(input);
    }
}
