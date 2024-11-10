namespace Accounting.API.Setup;

using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

public static class ServiceCollectionExtensions
{
    public static void AddLocal(this IServiceCollection services, IConfiguration configuration)
    {        
        var daprClient = new Dapr.Client.DaprClientBuilder().Build();
        
        services.AddDaprClient();        
        services.AddEndpointsApiExplorer();      
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
       
    }
}