using Testcontainers.PostgreSql;

namespace Nine.WebApi.Tests.Support;

internal static class TestHost
{
    private static PostgreSqlContainer? _postgres;
    private static WebApiFactory? _factory;

    public static WebApiFactory Factory =>
        _factory ?? throw new InvalidOperationException("The test host has not been started.");

    public static async Task StartAsync()
    {
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:17")
            .WithDatabase("nine_identity")
            .WithUsername("nine")
            .WithPassword("nine")
            .Build();

        await _postgres.StartAsync();

        Environment.SetEnvironmentVariable("ConnectionStrings__Identity", _postgres.GetConnectionString());

        _factory = new WebApiFactory(_postgres.GetConnectionString());
        _ = _factory.CreateClient();
    }

    public static async Task StopAsync()
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
