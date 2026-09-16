using System.Net;
using System.Text.Json;

using FluentAssertions;

using Nine.WebApi.Tests.Support;

using Reqnroll;

namespace Nine.WebApi.Tests.Bindings;

[Binding]
public sealed class CurrentUserSteps
{
    private readonly ApiContext _context;

    public CurrentUserSteps(ApiContext context)
    {
        _context = context;
    }

    [When("they request the current user")]
    public async Task WhenTheyRequestTheCurrentUser()
    {
        var api = new IdentitiesApi(_context.Client);
        await _context.CaptureAsync(await api.GetCurrentUserAsync(_context.AccessToken));
    }

    [When("someone requests the current user without a token")]
    public async Task WhenSomeoneRequestsTheCurrentUserWithoutAToken()
    {
        var api = new IdentitiesApi(_context.Client);
        await _context.CaptureAsync(await api.GetCurrentUserAsync(null));
    }

    [When("they request the current user with only the token header")]
    public async Task WhenTheyRequestTheCurrentUserWithOnlyTheTokenHeader()
    {
        var header = _context.AccessToken?.Split('.')[0];
        var api = new IdentitiesApi(_context.Client);
        await _context.CaptureAsync(await api.GetCurrentUserAsync(header));
    }

    [Then("the current user matches the registered identity")]
    public void ThenTheCurrentUserMatchesTheRegisteredIdentity()
    {
        _context.StatusCode.Should().Be(HttpStatusCode.OK);
        using var document = JsonDocument.Parse(_context.Body);
        document.RootElement.GetProperty("userId").GetString().Should().Be(_context.UserId);
        document.RootElement.GetProperty("email").GetString().Should().Be(_context.Email);
    }

    [Then("the request is unauthorized")]
    public void ThenTheRequestIsUnauthorized()
    {
        _context.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
