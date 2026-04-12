using Minimap.Core;
using Xunit;

namespace Minimap.Core.Tests;

public class GreetingTests
{
    [Fact]
    public void Hello_returns_expected_token()
    {
        Assert.Equal("minimap", Greeting.Hello());
    }
}
