using System.Linq.Expressions;
using System.Reflection;

namespace BeautifulCrud;

public sealed class ExpressionTarget(Expression expression, MethodInfo target)
{
    public Expression Expression { get; set; } = expression;
    public MethodInfo Target { get; set; } = target;
}