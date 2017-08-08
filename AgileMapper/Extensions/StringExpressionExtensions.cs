namespace AgileObjects.AgileMapper.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using NetStandardPolyfills;

    internal static class StringExpressionExtensions
    {
        private static readonly MethodInfo _stringJoinMethod = typeof(string)
            .GetPublicStaticMethods()
            .First(m => (m.Name == "Join") &&
                        (m.GetParameters().Length == 2) &&
                        (m.GetParameters()[1].ParameterType == typeof(string[])));

        private static readonly MethodInfo[] _stringConcatMethods = typeof(string)
            .GetPublicStaticMethods()
            .Where(m => m.Name == "Concat")
            .Select(m => new
            {
                Method = m,
                Parameters = m.GetParameters(),
                FirstParameterType = m.GetParameters().First().ParameterType
            })
            .Where(m => m.FirstParameterType == typeof(string))
            .OrderBy(m => m.Parameters.Length)
            .Select(m => m.Method)
            .ToArray();

        public static Expression GetStringConcatCall(this IList<Expression> expressions)
        {
            if (expressions.None())
            {
                return string.Empty.ToConstantExpression();
            }

            if (expressions.HasOne() && (expressions.First().NodeType == ExpressionType.Constant))
            {
                return expressions.First();
            }

            OptimiseForStringConcat(expressions);

            if (_stringConcatMethods.Length >= expressions.Count - 1)
            {
                var concatMethod = _stringConcatMethods[expressions.Count - 2];

                return Expression.Call(null, concatMethod, expressions);
            }

            var emptyString = Expression.Field(null, typeof(string), "Empty");
            var newStringArray = Expression.NewArrayInit(typeof(string), expressions);

            return Expression.Call(null, _stringJoinMethod, emptyString, newStringArray);
        }

        private static void OptimiseForStringConcat(IList<Expression> expressions)
        {
            if (expressions.HasOne())
            {
                return;
            }

            var currentNamePart = string.Empty;

            for (var i = expressions.Count - 1; i >= 0; --i)
            {
                var expression = expressions[i];

                if (expression.NodeType == ExpressionType.Constant)
                {
                    if ((i == 0) && (currentNamePart == string.Empty))
                    {
                        return;
                    }

                    currentNamePart = (string)((ConstantExpression)expression).Value + currentNamePart;
                    expressions.RemoveAt(i);
                    continue;
                }

                expressions.Insert(i + 1, currentNamePart.ToConstantExpression());
                currentNamePart = string.Empty;
            }

            expressions.Insert(0, currentNamePart.ToConstantExpression());
        }

        public static Expression GetLeftCall(this Expression stringAccess, int numberOfCharacters)
        {
            return Expression.Call(
                typeof(StringExtensions).GetPublicStaticMethod("Left"),
                stringAccess,
                numberOfCharacters.ToConstantExpression());
        }
    }
}