using Nine.WebApi.Configurations;
using Nine.WebApi.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentities();
builder.Services.AddMessaging();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program;
