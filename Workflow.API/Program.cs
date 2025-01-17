using Workflow.API.Models.Request;
using Workflow.API.Models.Response;
using Workflow.API.Setup;
using System.Text.Json.Serialization;


using Newtonsoft.Json;
using Microsoft.AspNetCore.SignalR.Client;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        builder =>
        {
            builder.WithOrigins("http://localhost:4200")
                .AllowAnyHeader()
                .WithMethods("GET", "POST")
                .AllowCredentials();
        });
});
builder.Services.AddLocal(builder.Configuration);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(6); // Set the keep-alive timeout to 4 minutes
    serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(60); // Set the request headers timeout to 30 seconds
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});


var app = builder.Build();

app.MapUserEndpoints();
app.UseCloudEvents();
app.UseCors();

HubConnection connection;
connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5269/hub/chat")
                .Build();
connection.Closed += async (error) =>
{
    await Task.Delay(new Random().Next(0, 5) * 1000);
    await connection.StartAsync();
};
connection.On<HubMessage>("CSRReceiveMessage", async (message) =>
{
    try
    {
       // var  msg=JsonConvert.DeserializeObject<HubMessage>(message.ToString());
        Console.WriteLine($"Message: {message.message}");
        Console.WriteLine("processing answer.......");

        var msg_chat = new Message { Messages = message.message };

        Console.WriteLine("CHAT MODE:" + message.mode);
        //enter logic here.


        string? value = "";
        using (HttpClient client = new HttpClient())
        {
            client.Timeout = TimeSpan.FromMinutes(3);
            if (message.mode.Equals("GroupChat"))
            {
                HttpResponseMessage resp = await client.PostAsJsonAsync<Message>("http://localhost:5006/groupchatraiseevent", msg_chat);
                resp.EnsureSuccessStatusCode();

                if (resp.IsSuccessStatusCode)
                {
                    string responseBody = await resp.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<Chato>(responseBody.ToString());
                    //JsonElement root = jsonDocument.RootElement;

                    //value = result.Conversation;// root.GetProperty("conversation").GetString();
                    //await connection.InvokeAsync("SendMessageToClient", String.Join(" ", value));
                }
                else
                {
                    Console.WriteLine($"Error: {resp.StatusCode}");
                }
            }
            else
            {
                HttpResponseMessage resp = await client.PostAsJsonAsync<Message>("http://localhost:5006/taskchat", msg_chat);
                resp.EnsureSuccessStatusCode();

                if (resp.IsSuccessStatusCode)
                {
                    string responseBody = await resp.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<Chato>(responseBody.ToString());
                    //JsonElement root = jsonDocument.RootElement;

                    //value = result.Conversation;// root.GetProperty("conversation").GetString();
                    //await connection.InvokeAsync("SendMessageToClient", String.Join(" ", value));
                }
                else
                {
                    Console.WriteLine($"Error: {resp.StatusCode}");
                }
            }
           
        }

    }
    catch (Exception ex)
    {
        Console.WriteLine("processing answer failed:" + ex.Message);
    }
});
try
{
    await connection.StartAsync();
    Console.WriteLine("Connection started");
}
catch (Exception ex)
{
    Console.WriteLine("Connection failed: " + ex.Message);
}

app.Run();

[JsonSerializable(typeof(Chat))]
[JsonSerializable(typeof(Message))]
//[JsonSerializable(typeof(Workflow.API.Features.Microagent.Handlers.ConversationHandlerRequest))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}
public record HubMessage(string message, string mode) { }

