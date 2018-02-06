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
            if (context.Settings.SupportsAllPrimitiveConstants ||
               (binary.NodeType != ExpressionType.Add) ||
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

            converted = Expression.Add(convertedLeft, convertedRight, _stringConcatStringsMethod);
            return true;
        }

        private static Expression ConvertOperand(Expression value)
        {
            if ((value.NodeType != ExpressionType.Convert) || (value.Type == typeof(string)))
            {
                return value;
            }

            var conversion = (UnaryExpression)value;

            return (conversion.Operand.NodeType == ExpressionType.Constant)
                ? ((ConstantExpression)conversion.Operand).Value.ToString().ToConstantExpression()
                : value;
        }
    }
}