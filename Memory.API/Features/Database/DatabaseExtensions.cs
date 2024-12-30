using Dapper;
using Pgvector.Dapper;

namespace Memory.API.Features.Database;

public static class DatabaseExtensions
{
    public static void AddDatabaseServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<DbContext>();
    }

    public static void ConfigureDatabaseServices(this WebApplication app)
    {
        SqlMapper.AddTypeHandler(new VectorTypeHandler());

        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DbContext>();
        Task.Run(() => context.Init()).Wait();
    }
}