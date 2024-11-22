using Workflow.API.Models.Request;
using Workflow.API.Models.Response;
using Workflow.API.Setup;
using System.Text.Json.Serialization;
using Dapr.Workflow;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateSlimBuilder(args);
builder.Services.AddLocal(builder.Configuration);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(4); // Set the keep-alive timeout to 4 minutes
    serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(60); // Set the request headers timeout to 30 seconds
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
//[JsonSerializable(typeof(Workflow.API.Features.Microagent.Handlers.ConversationHandlerRequest))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}

