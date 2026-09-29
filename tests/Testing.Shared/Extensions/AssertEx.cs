using Xunit;

namespace Testing.Shared.Extensions;

public static class AssertEx
{
    public static void Equal<T>(T expected, T actual, string message)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            Assert.Fail($"{message}{Environment.NewLine}Expected: {expected}{Environment.NewLine}Actual:   {actual}");
        }
    }
}
