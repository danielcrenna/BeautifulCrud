namespace BeautifulCrud;

public static class TypeExtensions
{
    public static bool IsNullableType(this Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
}