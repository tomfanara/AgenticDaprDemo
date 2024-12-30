
using Memory.API.Features.Database;
using Memory.API.SemanticKernel;
using Memory.API.Setup;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLocal(builder.Configuration);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen();
builder.AddDatabaseServices();
builder.AddSemanticKernelAsync();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();
