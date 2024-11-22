using Inventory.API.Models.Request;
using Inventory.API.Models.Response;
using Inventory.API.Setup;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);
builder.Services.AddLocal(builder.Configuration);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2); // Set the keep-alive timeout to 2 minutes
    serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(30); // Set the request headers timeout to 30 seconds
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var app = builder.Build();

app.MapUserEndpoints();
app.UseCloudEvents();

app.Run();

[JsonSerializable(typeof(Chat))]
[JsonSerializable(typeof(Message))]
[JsonSerializable(typeof(Inventory.API.Features.Microagent.Handlers.ConversationHandlerRequest))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}

