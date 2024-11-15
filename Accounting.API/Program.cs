using Accounting.API.Models.Request;
using Accounting.API.Models.Response;
using Accounting.API.Setup;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);
builder.Services.AddLocal(builder.Configuration);

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
[JsonSerializable(typeof(Accounting.API.Features.Microagent.Handlers.ConversationHandlerRequest))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}

