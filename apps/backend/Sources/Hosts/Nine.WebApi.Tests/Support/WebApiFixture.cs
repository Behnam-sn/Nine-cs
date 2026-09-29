using Microsoft.AspNetCore.Mvc.Testing;

using Testcontainers.PostgreSql;

namespace Nine.WebApi.Tests.Support;

public sealed class WebApiFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _postgres;
    private WebApiFactory? _factory;

    public HttpClient CreateClient()
    {
        var factory = _factory ?? throw new InvalidOperationException("The test host has not been started.");
        return factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    public async Task InitializeAsync()
    {
        _postgres = new PostgreSqlBuilder("postgres:17")
            .WithDatabase("nine_identity")
            .WithUsername("nine")
            .WithPassword("nine")
            .Build();

        await _postgres.StartAsync();

        var connectionString = _postgres.GetConnectionString();
        Environment.SetEnvironmentVariable("ConnectionStrings__Identity", connectionString);

        _factory = new WebApiFactory(connectionString);
        using var client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
            _factory = null;
        }

        if (_postgres is not null)
        {
            await _postgres.DisposeAsync();
            _postgres = null;
        }

        Environment.SetEnvironmentVariable("ConnectionStrings__Identity", null);
    }
}

[CollectionDefinition(WebApiCollection.Name)]
public sealed class WebApiCollection : ICollectionFixture<WebApiFixture>
{
    public const string Name = "WebApi";
}
