namespace AgileObjects.AgileMapper.Queryables.Converters
{
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions.Internal;
    using NetStandardPolyfills;

    internal static class StringConcatConverter
    {
        private static readonly MethodInfo _stringConcatObjectsMethod = typeof(string)
            .GetPublicStaticMethod("Concat", typeof(object), typeof(object));

        private static readonly MethodInfo _stringConcatStringsMethod =
            StringExpressionExtensions.GetConcatMethod(parameterCount: 2);

        public static bool TryConvert(
            BinaryExpression binary,
            IQueryProjectionModifier context,
            out Expression converted)
        {
            if ((binary.NodeType != ExpressionType.Add) ||
                !ReferenceEquals(binary.Method, _stringConcatObjectsMethod) ||
               ((binary.Left.NodeType != ExpressionType.Convert) && (binary.Right.NodeType != ExpressionType.Convert)))
            {
                converted = null;
                return false;
            }

            var convertedLeft = ConvertOperand(binary.Left);
            var convertedRight = ConvertOperand(binary.Right);

            if ((convertedLeft == binary.Left) && (convertedRight == binary.Right))
            {
                converted = null;
                return false;
            }

            if ((convertedLeft == null) || (convertedRight == null))
            {
                converted = convertedLeft ?? convertedRight;
                return true;
            }

            converted = Expression.Add(convertedLeft, convertedRight, _stringConcatStringsMethod);
            return true;
        }

        private static Expression ConvertOperand(Expression value)
        {
            while (true)
            {
                switch (value.NodeType)
                {
                    case ExpressionType.Constant:
                        var constant = ((ConstantExpression)value).Value;

                        return (constant == null)
                            ? null
                            : (value.Type != typeof(string))
                                ? constant.ToString().ToConstantExpression()
                                : ((string)constant != string.Empty) ? value : null;

                    case ExpressionType.Convert:
                        var conversion = (UnaryExpression)value;

                        if ((conversion.Operand.NodeType == ExpressionType.Constant))
                        {
                            value = conversion.Operand;
                            continue;
                        }

                        return value;

                    default:
                        return value;
                }
            }
        }
    }
}