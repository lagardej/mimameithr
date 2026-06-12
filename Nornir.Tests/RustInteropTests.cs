using System.Runtime.InteropServices;
using Xunit;

namespace Nornir.Tests;

public sealed class RustInteropTests
{
    [DllImport("seidr", CallingConvention = CallingConvention.Cdecl)]
    private static extern int test();

    [Fact]
    public void CallRustFunction_ReturnsExpectedValue()
    {
        var result = test();
        Assert.Equal(42, result);
    }
}
