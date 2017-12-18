﻿namespace AgileObjects.AgileMapper.Extensions.Internal
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using NetStandardPolyfills;

    internal static class StringExpressionExtensions
    {
        private static readonly MethodInfo _stringJoinMethod = typeof(string)
            .GetPublicStaticMethods("Join")
            .First(m => (m.GetParameters().Length == 2) &&
                        (m.GetParameters()[1].ParameterType == typeof(string[])));

        private static readonly MethodInfo[] _stringConcatMethods = typeof(string)
            .GetPublicStaticMethods("Concat")
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

            if (expressions.HasOne())
            {
                return expressions.First();
            }

            OptimiseForStringConcat(expressions);

            if (expressions.HasOne())
            {
                return expressions.First();
            }

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

        public static Expression GetFirstOrDefaultCall(this Expression stringAccess)
        {
            return Expression.Call(
                typeof(StringExtensions).GetPublicStaticMethod("FirstOrDefault"),
                stringAccess);
        }

        public static Expression GetMatchesKeyCall(
            this Expression stringAccess,
            Expression keyValue,
            Expression separator,
            Expression elementKeyPartMatcher)
        {
            if (separator == null)
            {
                return Expression.Call(
                    typeof(StringExtensions).GetPublicStaticMethod("MatchesKey", parameterCount: 2),
                    stringAccess,
                    keyValue);
            }

            if (elementKeyPartMatcher == null)
            {
                return GetMatchesKeyWithSeparatorCall(stringAccess, keyValue, separator);
            }

            var matcherPattern = ((Regex)((ConstantExpression)elementKeyPartMatcher).Value).ToString();

            if (matcherPattern == "[0-9]+")
            {
                // No prefix or suffix specified - removing the separator won't 
                // affect the element key parts:
                return GetMatchesKeyWithSeparatorCall(stringAccess, keyValue, separator);
            }

            return Expression.Call(
                typeof(StringExtensions).GetPublicStaticMethod("MatchesKey", parameterCount: 4),
                stringAccess,
                keyValue,
                separator,
                elementKeyPartMatcher);
        }

        private static Expression GetMatchesKeyWithSeparatorCall(Expression stringAccess, Expression keyValue,
            Expression separator)
        {
            return Expression.Call(
                typeof(StringExtensions).GetPublicStaticMethod("MatchesKey", parameterCount: 3),
                stringAccess,
                keyValue,
                separator);
        }
    }
}