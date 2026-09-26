using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Nine.WebApi.Tests.Support;

internal sealed class IdentityApi(HttpClient client)
{
    public Task<HttpResponseMessage> RegisterAsync(string email, string password, string? phoneNumber = null)
    {
        return client.PostAsJsonAsync(
            "/api/v1/UsersWebApi",
            new
            {
                emailAddress = email,
                password,
                phoneNumber
            });
    }

    public Task<HttpResponseMessage> SignInAsync(string email, string password)
    {
        return client.PostAsync(
            "/connect/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["username"] = email,
                ["password"] = password,
                ["client_id"] = "nine-resource-owner",
                ["scope"] = "openid email profile offline_access api"
            }));
    }

    public Task<HttpResponseMessage> GetCurrentUserAsync(string? accessToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/me");
        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        return client.SendAsync(request);
    }
}
