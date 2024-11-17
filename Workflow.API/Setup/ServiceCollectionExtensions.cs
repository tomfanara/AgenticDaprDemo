namespace Workflow.API.Setup;

using Dapr.Workflow;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Workflow.API.Features.Activities;
using Workflow.API.Models;

public static class ServiceCollectionExtensions
{
    public static void AddLocal(this IServiceCollection services, IConfiguration configuration)
    {        
        var daprClient = new Dapr.Client.DaprClientBuilder().Build();
        
        services.AddDaprClient();        
        services.AddEndpointsApiExplorer();      
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // register dapr services
        services.AddDaprWorkflow(options =>
        {
            options.RegisterWorkflow<TaskChainingWorkflow>();
            options.RegisterActivity<TaskChainingActivity>();
            options.RegisterActivity<TaskChainingCompensationActivity>();
        });

    }
}