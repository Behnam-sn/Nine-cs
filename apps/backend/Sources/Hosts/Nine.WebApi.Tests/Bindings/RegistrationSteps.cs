using System.Net;

using FluentAssertions;

using Nine.WebApi.Tests.Support;

using Reqnroll;

namespace Nine.WebApi.Tests.Bindings;

[Binding]
public sealed class RegistrationSteps
{
    private readonly ApiContext _context;

    public RegistrationSteps(ApiContext context)
    {
        _context = context;
    }

    [Given("a person with a unique email and password {string}")]
    public void GivenAPersonWithAUniqueEmailAndPassword(string password)
    {
        _context.Email = $"bdd.{Guid.NewGuid():N}@example.com";
        _context.Password = password;
    }

    [Given("a registered user")]
    public async Task GivenARegisteredUser()
    {
        GivenAPersonWithAUniqueEmailAndPassword("Password1!");
        await WhenTheyRegister();
        ThenRegistrationSucceeds();
    }

    [When("they register")]
    public async Task WhenTheyRegister()
    {
        var api = new IdentityApi(_context.Client);
        await _context.CaptureAsync(await api.RegisterAsync(_context.Email!, _context.Password!));
    }

    [When("they try to register again with the same email")]
    public async Task WhenTheyTryToRegisterAgainWithTheSameEmail()
    {
        var api = new IdentityApi(_context.Client);
        await _context.CaptureAsync(await api.RegisterAsync(_context.Email!, _context.Password!));
    }

    [Then("registration succeeds")]
    public void ThenRegistrationSucceeds()
    {
        _context.StatusCode.Should().Be(HttpStatusCode.Created);
        _context.UserId = _context.ReadJson<RegisterResponse>().UserId;
        _context.UserId.Should().NotBeNullOrWhiteSpace();
        Guid.TryParse(_context.UserId, out _).Should().BeTrue();
    }

    [Then("registration is rejected because the email is already in use")]
    public void ThenRegistrationIsRejectedBecauseTheEmailIsAlreadyInUse()
    {
        _context.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Then("registration is rejected as a bad request")]
    public void ThenRegistrationIsRejectedAsABadRequest()
    {
        _context.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private sealed record RegisterResponse(string UserId);
}
