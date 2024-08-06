using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Primitives;

namespace BeautifulCrud;

/// <summary>
/// See: https://github.com/microsoft/api-guidelines/blob/vNext/Guidelines.md#97-filtering
/// </summary>
public static partial class WhereParser
{
    private static readonly Regex GroupingRegex = GroupingCompiledRegex();

    [GeneratedRegex(@"\s*(\()|(\))|([^()]+)\s*", RegexOptions.Compiled)]
    private static partial Regex GroupingCompiledRegex();

    private static readonly Regex FilterRegex = FilterCompiledRegex();

    [GeneratedRegex("\\s*(?:([^\"\'\\s]+)|\'([^\']*)\'|\"([^\"]*)\")", RegexOptions.Compiled)]
    private static partial Regex FilterCompiledRegex();
    
    private static readonly Dictionary<string, LogicalOperator> LogicalOperators = new(StringComparer.OrdinalIgnoreCase)
    {
        ["and"] = LogicalOperator.And,
        ["or"] = LogicalOperator.Or,
        ["not"] = LogicalOperator.Not
    };

    private static readonly Dictionary<string, FilterOperator> FilterOperators = new(StringComparer.OrdinalIgnoreCase)
    {
        ["eq"] = FilterOperator.Equal,
        ["ne"] = FilterOperator.NotEqual,
        ["gt"] = FilterOperator.GreaterThan,
        ["ge"] = FilterOperator.GreaterThanOrEqual,
        ["lt"] = FilterOperator.LessThan,
        ["le"] = FilterOperator.LessThanOrEqual
    };

    public static IQueryable<T> BuildWhere<T>(this IQueryable<T> queryable, StringValues clauses) where T : class
    {
        var filter = Parse<T>(clauses);
        if (filter != null)
            queryable = queryable.Where(filter);
        return queryable;
    }

    private static Expression<Func<T, bool>>? Parse<T>(StringValues clauses)
    {
        var type = typeof(T);
        var context = new FilterContext();
        var parameter = Expression.Parameter(type, "x");

        var expressions = clauses.Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(clause => ParseFilter<T>(clause!, context, parameter))
            .OfType<Expression>()
            .ToList();
        
        if (expressions.Count <= 0)
            return null;

        {
            var expression = BuildExpression(context, expressions);
            return expression == null ? null : Expression.Lambda<Func<T, bool>>(expression, parameter);
        }
    }

    private static Expression? BuildExpression(FilterContext context, IReadOnlyList<Expression> expressions)
    {
        if (expressions.Count == 0) return null;

        var expression = expressions[0];
        for (var i = 1; i < expressions.Count; i++)
        {
            expression = context.LogicalOperatorStack[i - 1] switch
            {
                LogicalOperator.And => Expression.AndAlso(expression, expressions[i]),
                LogicalOperator.Or => Expression.OrElse(expression, expressions[i]),
                LogicalOperator.Not => Expression.AndAlso(expression, Expression.Not(expressions[i])),
                _ => throw new NotSupportedException($"Unsupported logical operator: {context.LogicalOperatorStack[i - 1]}")
            };
        }

        return expression;
    }
    
    private static void PushExpression(FilterContext context, Expression expression) => context.ExpressionStack.Add(expression);
    private static void PushLogicalOperator(FilterContext context, LogicalOperator logicalOperator) => context.LogicalOperatorStack.Add(logicalOperator);
    private static void PopLogicalOperator(FilterContext context) => context.LogicalOperatorStack.RemoveAt(context.LogicalOperatorStack.Count - 1);
    private static void PopExpressions(FilterContext context, int count) => context.ExpressionStack.RemoveRange(context.ExpressionStack.Count - count, count);
    
    private static bool TryParseLogicalOperator(string operatorString, out LogicalOperator logicalOperator) => LogicalOperators.TryGetValue(operatorString, out logicalOperator);
    private static bool TryParseFilterOperator(string operatorString, out FilterOperator filterOperator) => FilterOperators.TryGetValue(operatorString, out filterOperator);

    private static void ParseFilterClause<T>(string clause, FilterContext context, Expression parameter)
    {
        var matches = FilterRegex.Matches(clause);
        var tokens = matches.Select(m => m.Groups[1].Value + m.Groups[2].Value + m.Groups[3].Value).ToList();

        if (tokens.Count == 0)
            return;

        var type = typeof(T);
        var tokenIndex = 0;
        while (tokenIndex < tokens.Count)
        {
            var token = tokens[tokenIndex];

            if (TryParseLogicalOperator(token, out var logicalOperator))
            {
                PushLogicalOperator(context, logicalOperator);
                tokenIndex++;
            }
            else if (token == "(")
            {
                PushLogicalOperator(context, LogicalOperator.And);
                PushExpression(context, Expression.Constant("("));
                tokenIndex++;
            }
            else if (token == ")")
            {
                PopExpressions(context, 1);
                var expressionCount = context.ExpressionStack.Count;
                if (expressionCount > 0 &&
                    context.ExpressionStack[expressionCount - 1].NodeType == ExpressionType.Constant &&
                    ((ConstantExpression)context.ExpressionStack[expressionCount - 1]).Value!.Equals("("))
                    context.ExpressionStack.RemoveAt(expressionCount - 1);
                PopLogicalOperator(context);
                tokenIndex++;
            }
            else
            {
                if (tokenIndex + 2 >= tokens.Count)
                    break;

                if (TryParseFilterOperator(tokens[tokenIndex + 1], out var filterOperator))
                {
                    var name = tokens[tokenIndex];
                    var value = tokens[tokenIndex + 2].Trim('\'').Trim('"');

                    var propertyInfo =
                        type.GetProperty(name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (propertyInfo == null)
                        return;

                    var property = Expression.Property(parameter, propertyInfo.Name);
                    var propertyType = propertyInfo.PropertyType;

                    ConstantExpression constant;
                    if (propertyType == typeof(string))
                    {
                        constant = Expression.Constant(value, propertyType);
                    }
                    else
                    {
                        var valueType = propertyType.IsNullableType() ? Nullable.GetUnderlyingType(propertyType) : propertyType;
                        var converted = Convert.ChangeType(value, valueType!, CultureInfo.InvariantCulture);
                        constant = Expression.Constant(converted, propertyType);
                    }

                    BinaryExpression expression;
                    switch (filterOperator)
                    {
                        case FilterOperator.Equal:
                            expression = Expression.Equal(property, constant);
                            break;
                        case FilterOperator.NotEqual:
                            expression = Expression.NotEqual(property, constant);
                            break;
                        case FilterOperator.GreaterThan:
                            expression = Expression.GreaterThan(property, constant);
                            break;
                        case FilterOperator.GreaterThanOrEqual:
                            expression = Expression.GreaterThanOrEqual(property, constant);
                            break;
                        case FilterOperator.LessThan:
                            expression = Expression.LessThan(property, constant);
                            break;
                        case FilterOperator.LessThanOrEqual:
                            expression = Expression.LessThanOrEqual(property, constant);
                            break;
                        default:
                            return;
                    }

                    PushExpression(context, expression);
                    tokenIndex += 3;
                }
                else
                {
                    tokenIndex++;
                }
            }
        }
    }

    private static Expression? ParseFilter<T>(string filter, FilterContext context, Expression parameter)
    {
        var tokens = GroupingRegex.Split(filter);
        foreach (var token in tokens.Where(x => !string.IsNullOrWhiteSpace(x)))
            ParseFilterClause<T>(token, context, parameter);

        var expression = context.ExpressionStack.Count > 0 ? BuildExpression(context, context.ExpressionStack) : null;

        if (context.Debug && expression != null)
            Debug.WriteLine($"Generated expression tree: {ExpressionToString(expression)}");

        return expression;
    }

    private static string ExpressionToString(Expression expression)
    {
        var stringBuilder = new StringBuilder();
        var expressionWriter = new ExpressionWriter(stringBuilder);
        expressionWriter.Visit(expression);
        return stringBuilder.ToString();
    }
    
    public sealed class FilterContext
    {
        public List<Expression> ExpressionStack { get; set; } = [];
        public List<LogicalOperator> LogicalOperatorStack { get; set; } = [];
        public bool Debug { get; set; }

        public FilterContext()
        {
#if DEBUG
            Debug = true;
#endif
        }
    }

    private sealed class ExpressionWriter(StringBuilder stringBuilder) : ExpressionVisitor
    {
        public override Expression? Visit(Expression? node)
        {
            if (node == null)
                return null;

            switch (node.NodeType)
            {
                case ExpressionType.AndAlso:
                    var andAlsoExpression = (BinaryExpression) node;
                    stringBuilder.Append('(');
                    Visit(andAlsoExpression.Left);
                    stringBuilder.Append(" AND ");
                    Visit(andAlsoExpression.Right);
                    stringBuilder.Append(')');
                    break;
                case ExpressionType.OrElse:
                    var orElseExpression = (BinaryExpression) node;
                    stringBuilder.Append('(');
                    Visit(orElseExpression.Left);
                    stringBuilder.Append(" OR ");
                    Visit(orElseExpression.Right);
                    stringBuilder.Append(')');
                    break;
                case ExpressionType.Not:
                    var notExpression = (UnaryExpression) node;
                    stringBuilder.Append("NOT ");
                    Visit(notExpression.Operand);
                    break;
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                    var binaryExpression = (BinaryExpression) node;
                    Visit(binaryExpression.Left);
                    stringBuilder.Append(' ');
                    stringBuilder.Append(GetOperatorString(node.NodeType));
                    stringBuilder.Append(' ');
                    Visit(binaryExpression.Right);
                    break;
                case ExpressionType.Constant:
                    var constantExpression = (ConstantExpression) node;
                    stringBuilder.Append(constantExpression.Value);
                    break;
                case ExpressionType.MemberAccess:
                    var memberExpression = (MemberExpression) node;
                    stringBuilder.Append(memberExpression.Member.Name);
                    break;
                default:
                    return base.Visit(node);
            }

            return node;
        }

        private static string GetOperatorString(ExpressionType expressionType)
        {
            return expressionType switch
            {
                ExpressionType.Equal => "==",
                ExpressionType.NotEqual => "!=",
                ExpressionType.GreaterThan => ">",
                ExpressionType.GreaterThanOrEqual => ">=",
                ExpressionType.LessThan => "<",
                ExpressionType.LessThanOrEqual => "<=",
                _ => throw new NotSupportedException($"Unsupported expression type: {expressionType}")
            };
        }
    }
}