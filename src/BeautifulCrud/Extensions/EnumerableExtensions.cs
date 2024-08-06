namespace BeautifulCrud.Extensions;

internal static class EnumerableExtensions
{
    public static List<T> AsList<T>(this IEnumerable<T> value)
    {
        if (value is List<T> list)
            return list;
        return value.ToList();
    }
}