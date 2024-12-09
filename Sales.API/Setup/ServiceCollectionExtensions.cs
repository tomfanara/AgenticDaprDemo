namespace Sales.API.Setup;

using Microsoft.Extensions.DependencyInjection;
using Sales.API.Features.Microagent.Actors;
using System.Reflection;

public static class ServiceCollectionExtensions
{
    public static void AddLocal(this IServiceCollection services, IConfiguration configuration)
    {        
        var daprClient = new Dapr.Client.DaprClientBuilder().Build();
        
        services.AddDaprClient();        
        services.AddEndpointsApiExplorer();      
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddActors(options =>
        {
            // Register actor types and configure actor settings
            options.Actors.RegisterActor<SalesActor>();
            options.ReentrancyConfig = new Dapr.Actors.ActorReentrancyConfig()
            {
                Enabled = true,
                MaxStackDepth = 32,
            };
        });
    }
}