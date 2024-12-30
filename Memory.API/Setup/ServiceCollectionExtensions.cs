namespace Memory.API.Setup;

using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

public static class ServiceCollectionExtensions
{
    public static void AddLocal(this IServiceCollection services, IConfiguration configuration)
    {              
        services.AddEndpointsApiExplorer();      
             
    }
}