namespace AgileObjects.AgileMapper.Queryables.Converters
{
    using System.Reflection;
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal static class StringConcatConverter
    {
        private static readonly MethodInfo _stringConcatObjectsMethod = typeof(string)
            .GetPublicStaticMethod("Concat", typeof(object), typeof(object));

        private static readonly MethodInfo _stringConcatStringsMethod =
            StringExpressionExtensions.GetConcatMethod(parameterCount: 2);

        public static bool TryConvert(
            BinaryExpression binary,
            IQueryProjectionModifier modifier,
            out Expression converted)
        {
            if ((binary.NodeType != ExpressionType.Add) ||
                !ReferenceEquals(binary.Method, _stringConcatObjectsMethod) ||
               ((binary.Left.NodeType != ExpressionType.Convert) && (binary.Right.NodeType != ExpressionType.Convert)))
            {
                converted = null;
                return false;
            }

            var convertedLeft = ConvertOperand(binary.Left, modifier);
            var convertedRight = ConvertOperand(binary.Right, modifier);

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

        private static Expression ConvertOperand(Expression value, IQueryProjectionModifier modifier)
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

                        return conversion.Operand.Type == typeof(string)
                            ? conversion.Operand
                            : modifier.Modify(modifier
                                .MapperData
                                .GetValueConversion(conversion.Operand, typeof(string)));

                    default:
                        return value;
                }
            }
        }
    }
}