namespace BeautifulCrud.IntegrationTests.Extensions;

internal static class StringExtensions
{
    public static bool ContainsAny(this string input, IEnumerable<string> values, StringComparison comparison = StringComparison.Ordinal)
    {
        foreach (var value in values)
        {
            if (input.Contains(value, comparison))
                continue;
            return false;
        }

        return true;
    }
}