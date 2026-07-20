using Minimap.Extensive;
using Xunit;

namespace Minimap.App.Tests;

public class ExtensionRegistryTests
{
    [Fact]
    public void AddIntegrator_registers_by_id()
    {
        var registry = new ExtensionRegistry();
        registry.AddIntegrator(new DefaultIntegrator());

        Assert.True(registry.TryGetIntegrator("default", out var integrator));
        Assert.Equal("default", integrator!.Id);
        Assert.Single(registry.Integrators);
    }

    [Fact]
    public void AddIntegrator_rejects_duplicate_id()
    {
        var registry = new ExtensionRegistry();
        registry.AddIntegrator(new DefaultIntegrator());

        Assert.Throws<InvalidOperationException>(() =>
            registry.AddIntegrator(new DefaultIntegrator()));
    }

    [Fact]
    public void RequireIntegrator_throws_when_missing()
    {
        var registry = new ExtensionRegistry();
        Assert.Throws<InvalidOperationException>(() => registry.RequireIntegrator("missing"));
    }
}
