using Nine.Identity.Presentation.Authentication.WebApi;
using Nine.Identity.Presentation.Authentication.WebApi.Endpoints;
using Nine.Identity.Presentation.Users.WebApi;
using Nine.Profiles.Presentation.Profiles.WebApi;
using Nine.Profiles.Presentation.Profiles.WebApi.Endpoints;
using Nine.WebApi.Configurations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentity();
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
app.MapUsersWebApi();
app.MapAuthorizationWebApi();
app.MapProfilesWebApi();

app.Run();

namespace Nine.WebApi
{
    public partial class Program;
}
