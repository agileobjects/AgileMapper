namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Linq.Expressions;
    using Extensions;
    using TypeConversion;

    internal class EnumMemberPair
    {
        private readonly Type _firstEnumType;
        private readonly Type _secondEnumType;

        public EnumMemberPair(
            Type firstEnumType,
            string firstEnumMemberName,
            Type secondEnumType,
            string secondEnumMemberName,
            IValueConverter valueConverter)
        {
            FirstEnumMemberName = firstEnumMemberName;
            SecondEnumMemberName = secondEnumMemberName;
            ValueConverter = valueConverter;
            _firstEnumType = firstEnumType;
            _secondEnumType = secondEnumType;
        }

        public static EnumMemberPair For<TFirstEnum, TSecondEnum>(
            TFirstEnum firstEnumMember,
            TSecondEnum secondEnumMember)
        {
            var firstValue = firstEnumMember.ToConstantExpression();
            var secondValue = secondEnumMember.ToConstantExpression();

            var valueConverter = new ConfiguredValueConverter(
                firstValue.Type,
                secondValue,
                (sourceValue, targetType) => Expression.Equal(sourceValue, firstValue));

            return new EnumMemberPair(
                firstValue.Type,
                firstEnumMember.ToString(),
                secondValue.Type,
                secondEnumMember.ToString(),
                valueConverter);
        }

        public string FirstEnumMemberName { get; }

        public string SecondEnumMemberName { get; }

        public IValueConverter ValueConverter { get; }

        public bool IsFor(Type sourceEnumType, Type targetEnumType)
            => (_firstEnumType == sourceEnumType) && (_secondEnumType == targetEnumType);
    }
}