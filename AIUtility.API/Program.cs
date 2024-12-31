using System.Text.Json.Serialization;
using AIUtility.API.Models.Request;
using AIUtility.API.Models.Response;
using AIUtility.API.Setup;

var builder = WebApplication.CreateSlimBuilder(args);
builder.Services.AddLocal(builder.Configuration);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(6); // Set the keep-alive timeout to 6 minutes
    serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(60); // Set the request headers timeout to 60 seconds
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var app = builder.Build();

app.MapUserEndpoints();

app.Run();

[JsonSerializable(typeof(Chat))]
[JsonSerializable(typeof(Message))]
//[JsonSerializable(typeof(Workflow.API.Features.Microagent.Handlers.ConversationHandlerRequest))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}
