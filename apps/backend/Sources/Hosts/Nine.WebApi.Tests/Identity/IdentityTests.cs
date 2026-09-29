using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using FluentAssertions;

using Nine.WebApi.Tests.Support;

namespace Nine.WebApi.Tests.Identity;

[Collection(WebApiCollection.Name)]
public sealed class IdentityTests(WebApiFixture fixture)
{
    private const string Password = "Password1!";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public async Task Register_ThenSignIn_ReturnsCurrentUser()
    {
        // Arrange
        var email = UniqueEmail();
        using var client = fixture.CreateClient();
        var api = new IdentityApi(client);

        // Act
        var userId = await RegisterAsync(api, email, Password);
        var accessToken = await SignInAsync(api, email, Password);
        var currentUser = await api.GetCurrentUserAsync(accessToken);

        // Assert
        currentUser.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await currentUser.Content.ReadFromJsonAsync<CurrentUserResponse>(JsonOptions);
        body!.UserId.Should().Be(userId);
        body.Email.Should().Be(email);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        // Arrange
        var email = UniqueEmail();
        using var client = fixture.CreateClient();
        var api = new IdentityApi(client);
        await RegisterAsync(api, email, Password);

        // Act
        var response = await api.RegisterAsync(email, Password);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_WeakPassword_ReturnsBadRequest()
    {
        // Arrange
        using var client = fixture.CreateClient();
        var api = new IdentityApi(client);

        // Act
        var response = await api.RegisterAsync(UniqueEmail(), "short");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SignIn_WrongPassword_ReturnsInvalidGrant()
    {
        // Arrange
        var email = UniqueEmail();
        using var client = fixture.CreateClient();
        var api = new IdentityApi(client);
        await RegisterAsync(api, email, Password);

        // Act
        var response = await api.SignInAsync(email, "WrongPass1!");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Forbidden);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        document.RootElement.GetProperty("error").GetString().Should().Be("invalid_grant");
    }

    [Fact]
    public async Task GetCurrentUser_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        using var client = fixture.CreateClient();
        var api = new IdentityApi(client);

        // Act
        var response = await api.GetCurrentUserAsync(null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUser_TruncatedToken_ReturnsUnauthorized()
    {
        // Arrange
        var email = UniqueEmail();
        using var client = fixture.CreateClient();
        var api = new IdentityApi(client);
        await RegisterAsync(api, email, Password);
        var accessToken = await SignInAsync(api, email, Password);

        // Act
        var response = await api.GetCurrentUserAsync(accessToken.Split('.')[0]);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static async Task<string> RegisterAsync(IdentityApi api, string email, string password)
    {
        var response = await api.RegisterAsync(email, password);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<RegisterResponse>(JsonOptions);
        body!.UserId.Should().NotBeNullOrWhiteSpace();
        Guid.TryParse(body.UserId, out _).Should().BeTrue();
        return body.UserId;
    }

    private static async Task<string> SignInAsync(IdentityApi api, string email, string password)
    {
        var response = await api.SignInAsync(email, password);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<TokenResponse>(JsonOptions);
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.AccessToken.Split('.').Should().HaveCount(3);
        return body.AccessToken;
    }

    private static string UniqueEmail()
    {
        return $"user.{Guid.NewGuid():N}@example.com";
    }

    private sealed record RegisterResponse(string UserId);

    private sealed record TokenResponse([property: JsonPropertyName("access_token")] string AccessToken);

    private sealed record CurrentUserResponse(string UserId, string? Email);
}
