using Nine.WebApi.Tests.Support;

using Reqnroll;

namespace Nine.WebApi.Tests.Bindings;

[Binding]
public sealed class Hooks
{
    private readonly ApiContext _context;

    public Hooks(ApiContext context)
    {
        _context = context;
    }

    [BeforeTestRun]
    public static Task BeforeTestRun()
    {
        return TestHost.StartAsync();
    }

    [AfterTestRun]
    public static Task AfterTestRun()
    {
        return TestHost.StopAsync();
    }

    [BeforeScenario]
    public void BeforeScenario()
    {
        _context.Client = TestHost.Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [AfterScenario]
    public void AfterScenario()
    {
        _context.Client.Dispose();
    }
}
