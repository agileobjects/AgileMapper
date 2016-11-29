namespace AgileObjects.AgileMapper.Api.Configuration
{
    using AgileMapper.Configuration;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;

    /// <summary>
    /// Provides options for specifying the enum member to which the configured enum member should be paired.
    /// </summary>
    /// <typeparam name="TFirstEnum">The type of the first enum being paired.</typeparam>
    public class EnumPairSpecifier<TFirstEnum>
    {
        private readonly MappingConfigInfo _configInfo;
        private readonly TFirstEnum _firstEnumMember;

        private EnumPairSpecifier(
            MapperContext mapperContext,
            TFirstEnum firstEnumMember)
        {
            _configInfo = MappingConfigInfo.AllRuleSetsSourceTypesAndTargetTypes(mapperContext);
            _firstEnumMember = firstEnumMember;
        }

        #region Factory Method

        internal static EnumPairSpecifier<TFirstEnum> For(
            MapperContext mapperContext,
            TFirstEnum firstEnumMember)
        {
            ThrowIfNotEnumType<TFirstEnum>();

            return new EnumPairSpecifier<TFirstEnum>(mapperContext, firstEnumMember);
        }

        private static void ThrowIfNotEnumType<T>()
        {
            if (!typeof(T).IsEnum())
            {
                throw new MappingConfigurationException(
                    typeof(T).GetFriendlyName() + " is not an enum type.");
            }
        }

        #endregion

        /// <summary>
        /// Configure this mapper to map the specified first enum member to the given <paramref name="secondEnumMember"/>.
        /// </summary>
        /// <typeparam name="TSecondEnum">The type of the second enum being paired.</typeparam>
        /// <param name="secondEnumMember">The second enum member in the pair.</param>
        /// <returns>A MappingConfigContinuation to enable further global mapping configuration.</returns>
        public MappingConfigContinuation<object, object> With<TSecondEnum>(TSecondEnum secondEnumMember)
            where TSecondEnum : struct
        {
            ThrowIfNotEnumType<TSecondEnum>();
            ThrowIfSameTypes<TSecondEnum>();

            var firstToSecondPairing = EnumMemberPair.For(_firstEnumMember, secondEnumMember);
            var secondToFirstPairing = EnumMemberPair.For(secondEnumMember, _firstEnumMember);

            _configInfo.MapperContext.ValueConverters.Add(firstToSecondPairing.ValueConverter);
            _configInfo.MapperContext.ValueConverters.Add(secondToFirstPairing.ValueConverter);

            _configInfo.MapperContext.UserConfigurations.Add(firstToSecondPairing);
            _configInfo.MapperContext.UserConfigurations.Add(secondToFirstPairing);

            return new MappingConfigContinuation<object, object>(_configInfo);
        }

        private static void ThrowIfSameTypes<TSecondEnum>()
        {
            if (typeof(TFirstEnum) == typeof(TSecondEnum))
            {
                throw new MappingConfigurationException(
                    "Enum pairing can only be configured between different enum types.");
            }
        }
    }
}