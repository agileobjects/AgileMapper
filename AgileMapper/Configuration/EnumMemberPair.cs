namespace AgileObjects.AgileMapper.Configuration
{
    using System.Linq.Expressions;
    using TypeConversion;

    internal class EnumMemberPair : UserConfiguredItemBase
    {
        private readonly object _firstEnumMember;
        private readonly object _secondEnumMember;

        public EnumMemberPair(
            MappingConfigInfo configInfo,
            object firstEnumMember,
            object secondEnumMember)
            : base(configInfo)
        {
            _firstEnumMember = firstEnumMember;
            _secondEnumMember = secondEnumMember;
        }

        public static EnumMemberPair For<TFirstEnum, TSecondEnum>(
            MappingConfigInfo configInfo,
            TFirstEnum firstEnumMember,
            TSecondEnum secondEnumMember)
        {
            var firstValue = Expression.Constant(firstEnumMember, typeof(TFirstEnum));
            var secondValue = Expression.Constant(secondEnumMember, typeof(TSecondEnum));

            var firstToSecondValueConverter = new ConfiguredValueConverter(
                firstValue.Type,
                secondValue,
                (sourceValue, targetType) => Expression.Equal(sourceValue, firstValue));

            var secondToFirstValueConverter = new ConfiguredValueConverter(
                secondValue.Type,
                firstValue,
                (sourceValue, targetType) => Expression.Equal(sourceValue, secondValue));

            configInfo.MapperContext.ValueConverters.Add(firstToSecondValueConverter);
            configInfo.MapperContext.ValueConverters.Add(secondToFirstValueConverter);

            return new EnumMemberPair(configInfo, firstEnumMember, secondEnumMember);
        }
    }
}