using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

using FluentAssertions;

using Nine.WebApi.Tests.Support;

using Reqnroll;

namespace Nine.WebApi.Tests.Bindings;

[Binding]
public sealed class AuthenticationSteps
{
    private readonly ApiContext _context;

    public AuthenticationSteps(ApiContext context)
    {
        _context = context;
    }

    [Given("a registered user who has signed in")]
    public async Task GivenARegisteredUserWhoHasSignedIn()
    {
        var registration = new RegistrationSteps(_context);
        await registration.GivenARegisteredUser();
        await WhenTheySignInWithThePasswordGrant();
        ThenTheyReceiveAnAccessToken();
    }

    [When("they sign in with the password grant")]
    public async Task WhenTheySignInWithThePasswordGrant()
    {
        var api = new IdentitiesApi(_context.Client);
        await _context.CaptureAsync(await api.SignInAsync(_context.Email!, _context.Password!));
    }

    [When("they sign in with password {string}")]
    public async Task WhenTheySignInWithPassword(string password)
    {
        var api = new IdentitiesApi(_context.Client);
        await _context.CaptureAsync(await api.SignInAsync(_context.Email!, password));
    }

    [Then("they receive an access token")]
    public void ThenTheyReceiveAnAccessToken()
    {
        _context.StatusCode.Should().Be(HttpStatusCode.OK);
        _context.AccessToken = _context.ReadJson<TokenResponse>().AccessToken;
        _context.AccessToken.Should().NotBeNullOrWhiteSpace();
        _context.AccessToken.Split('.').Should().HaveCount(3);
    }

    [Then("sign in is rejected")]
    public void ThenSignInIsRejected()
    {
        _context.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Forbidden);
        using var document = JsonDocument.Parse(_context.Body);
        document.RootElement.GetProperty("error").GetString().Should().Be("invalid_grant");
    }

    private sealed record TokenResponse([property: JsonPropertyName("access_token")] string AccessToken);
}
