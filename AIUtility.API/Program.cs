#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0003
#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0011
#pragma warning disable SKEXP0050
#pragma warning disable SKEXP0052
#pragma warning disable SKEXP0070

using System.Text.Json;
using System.Text.Json.Serialization;
using AIUtility.API.Models.Request;
using AIUtility.API.Models.Response;
using AIUtility.API.Setup;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Memory;
using Microsoft.SemanticKernel.Text;
using Newtonsoft.Json;

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
var SKbuilder = Kernel.CreateBuilder()
                     .AddOllamaChatCompletion(
                      modelId: "llama3",
                      endpoint: new Uri("http://localhost:11434"));
SKbuilder.AddLocalTextEmbeddingGeneration();
Kernel kernel = SKbuilder.Build();

//HubConnection connection;
//connection = new HubConnectionBuilder()
//                .WithUrl("http://localhost:5269/hub/chat")
//                .Build();
//connection.Closed += async (error) =>
//{
//    await Task.Delay(new Random().Next(0, 5) * 1000);
//    await connection.StartAsync();
//};
//connection.On<string>("CRSReceiveMessage", async (message) =>
//{
//    try
//    {
//        Console.WriteLine($"Message: {message}");
//        Console.WriteLine("processing answer.......");

//        var msg = new Message { Messages = message };
//        //string? value = "";
//        using (HttpClient client = new HttpClient())
//        {
//            client.Timeout = TimeSpan.FromMinutes(3);
//            HttpResponseMessage resp = await client.PostAsJsonAsync<Message>("http://localhost:5167/converse", msg);
//            resp.EnsureSuccessStatusCode();

//            if (resp.IsSuccessStatusCode)
//            {
//                string responseBody = await resp.Content.ReadAsStringAsync();
//                var result = JsonConvert.DeserializeObject<Chato>(responseBody.ToString());
//                //JsonElement root = jsonDocument.RootElement;

//                var value = result.Conversation;// root.GetProperty("conversation").GetString();
//                await connection.InvokeAsync("SendMessageToClient", String.Join(" ", value));
//            }
//            else
//            {
//                Console.WriteLine($"Error: {resp.StatusCode}");
//            }
//        }

//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine("processing answer failed:" + ex.Message);
//    }
//});
//try
//{
//    await connection.StartAsync();
//    Console.WriteLine("Connection started");
//}
//catch (Exception ex)
//{
//    Console.WriteLine("Connection failed: "+ex.Message);
//}

app.Run();

[JsonSerializable(typeof(Chat))]
[JsonSerializable(typeof(Message))]
//[JsonSerializable(typeof(Workflow.API.Features.Microagent.Handlers.ConversationHandlerRequest))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}
