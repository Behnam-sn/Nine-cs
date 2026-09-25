using System.Net;
using System.Text.Json;

namespace Nine.WebApi.Tests.Support;

public sealed class ApiContext
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public HttpClient Client { get; set; } = null!;
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? UserId { get; set; }
    public string? AccessToken { get; set; }
    public HttpStatusCode? StatusCode { get; private set; }
    public string Body { get; private set; } = string.Empty;

    public async Task CaptureAsync(HttpResponseMessage response)
    {
        StatusCode = response.StatusCode;
        Body = await response.Content.ReadAsStringAsync();
        response.Dispose();
    }

    public T ReadJson<T>()
    {
        return JsonSerializer.Deserialize<T>(Body, JsonOptions)
               ?? throw new InvalidOperationException("The response body was empty.");
    }
}
