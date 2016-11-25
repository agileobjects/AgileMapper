namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Extensions;
    using NetStandardPolyfills;

    internal class EnumComparisonFixer : ExpressionVisitor
    {
        private readonly Dictionary<Expression, Expression> _comparisonReplacements;

        public EnumComparisonFixer()
        {
            _comparisonReplacements = new Dictionary<Expression, Expression>();
        }

        public static LambdaExpression Check(LambdaExpression lambda)
        {
            var fixer = new EnumComparisonFixer();

            fixer.Visit(lambda.Body);

            var updatedLambda = lambda.Replace(fixer._comparisonReplacements);

            return updatedLambda;
        }

        protected override Expression VisitBinary(BinaryExpression binary)
        {
            if ((binary.NodeType == ExpressionType.Equal) || (binary.NodeType == ExpressionType.NotEqual))
            {
                Expression enumMember, enumValue;

                if ((TryGetConvertedEnumMember(binary.Left, out enumMember) &&
                     TryGetConstantEnumValue(binary.Right, enumMember, out enumValue)) ||
                    (TryGetConvertedEnumMember(binary.Right, out enumMember) &&
                     TryGetConstantEnumValue(binary.Left, enumMember, out enumValue)))
                {
                    var enumComparison = binary.Update(enumMember, binary.Conversion, enumValue);

                    _comparisonReplacements.Add(binary, enumComparison);
                }

            }

            return base.VisitBinary(binary);
        }

        private static bool TryGetConvertedEnumMember(Expression value, out Expression enumMember)
        {
            if (value.NodeType != ExpressionType.Convert)
            {
                enumMember = null;
                return false;
            }

            var convertedValue = ((UnaryExpression)value).Operand;
            var valueNonNullableType = convertedValue.Type.GetNonNullableType();

            if (!valueNonNullableType.IsEnum())
            {
                enumMember = null;
                return false;
            }

            if (valueNonNullableType != convertedValue.Type)
            {
                // The enum member being compared to an enum constant is nullable;
                // the NestedAccessFinder will weave in a HasValue check, so we can 
                // use its .Value property in the fixed comparison:
                convertedValue = Expression.Property(convertedValue, "Value");
            }

            enumMember = convertedValue;
            return true;
        }

        private static bool TryGetConstantEnumValue(
            Expression value,
            Expression enumMember,
            out Expression enumValue)
        {
            Type enumType, expectedValueType;

            if (value.NodeType == ExpressionType.Convert)
            {
                enumType = expectedValueType = enumMember.Type.GetNonNullableType();
                value = ((UnaryExpression)value).Operand;
            }
            else
            {
                enumType = enumMember.Type;
                expectedValueType = Enum.GetUnderlyingType(enumType);
            }

            if ((value.NodeType != ExpressionType.Constant) || (value.Type != expectedValueType))
            {
                enumValue = null;
                return false;
            }

            var enumConstant = (ConstantExpression)value;
            var enumMemberName = Enum.GetName(enumType, enumConstant.Value);
            enumValue = Expression.Field(null, enumType, enumMemberName);
            return true;
        }
    }
}