using System.Linq.Expressions;
using System.Reflection;

namespace BeautifulCrud;

internal static class SelectParser
{
    public static Expression<Func<T, T>> BuildSelect<T>(ProjectionPath[] paths)
    {
        var parameter = Expression.Parameter(typeof(T));
        var memberBindings = new List<MemberBinding>();
        var propertyAccesses = new Dictionary<string, Expression>();

        var topLevelPaths = paths.Where(p => p.Next == null).ToArray();
        var nestedPaths = paths.Where(p => p.Next != null).ToArray();

        foreach (var path in topLevelPaths)
        {
            var property = typeof(T).GetProperty(path.Name!, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (property == null)
                continue;

            var propertyAccess = Expression.Property(parameter, property);
            ProcessPath(property, propertyAccess, path.Next);
        }

        foreach (var path in nestedPaths)
        {
            var property = typeof(T).GetProperty(path.Name!, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (property == null)
                continue;

            var propertyAccess = Expression.Property(parameter, property);
            propertyAccesses[property.Name] = propertyAccess;

            ProcessPath(property, propertyAccess, path.Next);
        }

        var constructor = SelectConstructor<T>(propertyAccesses.Keys);
        var arguments = constructor.GetParameters()
	        .Select(p => propertyAccesses.TryGetValue(p.Name!, out var expression) ? expression : Expression.Default(p.ParameterType))
	        .ToArray();

        var newExpression = Expression.New(constructor, arguments);

        var memberInitExpression = Expression.MemberInit(newExpression, memberBindings);
        var selectorLambda = Expression.Lambda<Func<T, T>>(memberInitExpression, parameter);

        return selectorLambda;

        void ProcessPath(PropertyInfo? property, Expression currentExpression, ProjectionPath? remainingPath)
        {
            if (property == null || !property.CanWrite)
                return;

            var isCollection = property.PropertyType.GetInterfaces()
                .Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (!isCollection || remainingPath == null)
                memberBindings.Add(Expression.Bind(property, currentExpression));

            if (remainingPath == null)
                return;

            var elementType = isCollection
                ? property.PropertyType.GetGenericArguments().FirstOrDefault()
                : property.PropertyType;

            if (elementType == null)
                return;

            var innerLambdaMethodInfo = typeof(SelectParser)
                .GetMethod(nameof(BuildSelect), BindingFlags.Static | BindingFlags.Public)!
                .MakeGenericMethod(elementType);

            var innerLambda = (LambdaExpression)innerLambdaMethodInfo.Invoke(null, [new[] { remainingPath }])!;
            var selectMethod = typeof(Enumerable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m => m.Name == "Select" && m.GetParameters().Length == 2)!
                .MakeGenericMethod(elementType, elementType);

            Expression projectedProperty;

            if (isCollection)
            {
                currentExpression = Expression.Convert(currentExpression, typeof(IEnumerable<>).MakeGenericType(elementType));
                projectedProperty = Expression.Call(selectMethod, currentExpression, innerLambda);
            }
            else
            {
                projectedProperty = Expression.Invoke(innerLambda, currentExpression);
            }

            memberBindings.Add(Expression.Bind(property, projectedProperty));
        }
    }

    private static ConstructorInfo SelectConstructor<T>(IEnumerable<string> availablePropertyNames)
    {
	    var constructors = typeof(T).GetConstructors();
	    var selectedConstructor = constructors
		    .OrderByDescending(c => c.GetParameters().Count(p => availablePropertyNames.Contains(p.Name, StringComparer.OrdinalIgnoreCase)))
		    .First();
	    return selectedConstructor;
    }
}