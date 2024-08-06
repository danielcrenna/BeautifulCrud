using System.Linq.Expressions;
using System.Reflection;

namespace BeautifulCrud;

public static class OrderByParser
{
    public static IQueryable<T> BuildSort<T>(this IQueryable<T> queryable, string path, bool isDescending, bool isFirstOrder)
    {
        var invocation = CreateNestedOrderBy<T>(path, isDescending, isFirstOrder);
        var invoked = invocation.Target.Invoke(null, [queryable, invocation.Expression]);
        var orderedQueryable = (IQueryable<T>)invoked!;
        return orderedQueryable;
    }

    private static ExpressionTarget CreateNestedOrderBy<T>(string path, bool isDescending, bool isFirstOrder)
    {
        var entityType = typeof(T);
        var currentType = entityType;

        var parameter = Expression.Parameter(entityType, "x");
        Expression propertyExpression = parameter;

        foreach (var propertyName in path.Split('.'))
        {
            var property = currentType.GetProperty(propertyName,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (property == null)
                throw new ArgumentException($"Property {propertyName} not found on type {currentType.Name}");

            currentType = property.PropertyType;
            propertyExpression = Expression.Property(propertyExpression, property);
        }

        var propertyType = propertyExpression.Type;
        var lambda = Expression.Lambda(propertyExpression, parameter);

        var methodName = isFirstOrder
            ? isDescending ? "OrderByDescending" : "OrderBy"
            : isDescending
                ? "ThenByDescending"
                : "ThenBy";

        var method = typeof(Queryable).GetMethods()
            .FirstOrDefault(m => m.Name == methodName && m.GetParameters().Length == 2)
            !.MakeGenericMethod(entityType, propertyType);

        return new ExpressionTarget(lambda, method);
    }
}