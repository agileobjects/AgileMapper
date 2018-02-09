namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using AgileMapper.Configuration;
    using Extensions.Internal;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;

    /// <summary>
    /// Provides options for specifying the enum member to which the configured enum member should be paired.
    /// </summary>
    /// <typeparam name="TSource">The source type being configured.</typeparam>
    /// <typeparam name="TTarget">The target type being configured.</typeparam>
    /// <typeparam name="TPairingEnum">The type of the first enum being paired.</typeparam>
    public class EnumPairSpecifier<TSource, TTarget, TPairingEnum>
    {
        private readonly MappingConfigInfo _configInfo;
        private readonly TPairingEnum[] _pairingEnumMembers;

        private EnumPairSpecifier(
            MappingConfigInfo configInfo,
            TPairingEnum[] pairingEnumMembers)
        {
            _configInfo = configInfo;
            _pairingEnumMembers = pairingEnumMembers;
        }

        #region Factory Method

        internal static EnumPairSpecifier<TSource, TTarget, TPairingEnum> For(
            MappingConfigInfo configInfo,
            params TPairingEnum[] firstEnumMembers)
        {
            ThrowIfNotEnumType<TPairingEnum>();
            ThrowIfEmpty(firstEnumMembers);

            return new EnumPairSpecifier<TSource, TTarget, TPairingEnum>(configInfo, firstEnumMembers);
        }

        private static void ThrowIfNotEnumType<T>()
        {
            if (!typeof(T).IsEnum())
            {
                throw new MappingConfigurationException(
                    typeof(T).GetFriendlyName() + " is not an enum type.");
            }
        }

        private static void ThrowIfEmpty(ICollection<TPairingEnum> firstEnumMembers)
        {
            if (firstEnumMembers.None())
            {
                throw new MappingConfigurationException("Source enum members must be provided.");
            }
        }

        #endregion

        private MapperContext MapperContext => _configInfo.MapperContext;

        /// <summary>
        /// Configure this mapper to map the specified first enum member to the given <paramref name="pairedEnumMember"/>.
        /// </summary>
        /// <typeparam name="TPairedEnum">The type of the second enum being paired.</typeparam>
        /// <param name="pairedEnumMember">The second enum member in the pair.</param>
        /// <returns>A MappingConfigContinuation with which to configure other aspects of mapping.</returns>
        public MappingConfigContinuation<TSource, TTarget> With<TPairedEnum>(TPairedEnum pairedEnumMember)
            where TPairedEnum : struct
        {
            return PairEnums(pairedEnumMember);
        }

        /// <summary>
        /// Configure this mapper to map the previously-specified set of enum members to the given 
        /// <paramref name="pairedEnumMembers"/>.
        /// </summary>
        /// <typeparam name="TPairedEnum">The type of the second enum being paired.</typeparam>
        /// <param name="pairedEnumMembers">The second set of enum members in the pairs.</param>
        /// <returns>A MappingConfigContinuation with which to configure other aspects of mapping.</returns>
        public MappingConfigContinuation<TSource, TTarget> With<TPairedEnum>(params TPairedEnum[] pairedEnumMembers)
            where TPairedEnum : struct
        {
            return PairEnums(pairedEnumMembers);
        }

        private MappingConfigContinuation<TSource, TTarget> PairEnums<TPairedEnum>(params TPairedEnum[] pairedEnumMembers)
        {
            ThrowIfNotEnumType<TPairedEnum>();
            ThrowIfSameTypes<TPairedEnum>();
            ThrowIfEmpty(pairedEnumMembers);
            ThrowIfIncompatibleNumbers(pairedEnumMembers);

            var hasSinglePairedEnumValue = pairedEnumMembers.Length == 1;
            var firstPairedEnumMember = hasSinglePairedEnumValue ? pairedEnumMembers[0] : default(TPairedEnum);
            var createReversePairings = _pairingEnumMembers.Length == pairedEnumMembers.Length;

            for (var i = 0; i < _pairingEnumMembers.Length; i++)
            {
                var pairingEnumMember = _pairingEnumMembers[i];
                var pairedEnumMember = hasSinglePairedEnumValue ? firstPairedEnumMember : pairedEnumMembers[i];

                ThrowIfAlreadyPaired<TPairedEnum>(pairingEnumMember);

                ConfigureEnumPair(pairingEnumMember, pairedEnumMember);

                if (createReversePairings && ValueIsNotAlreadyPaired(pairedEnumMember))
                {
                    ConfigureEnumPair(pairedEnumMember, pairingEnumMember);
                }
            }

            return new MappingConfigContinuation<TSource, TTarget>(_configInfo);
        }

        private void ConfigureEnumPair<TPairing, TPaired>(TPairing pairingEnumMember, TPaired pairedEnumMember)
        {
            var pairing = EnumMemberPair.For(pairingEnumMember, pairedEnumMember);

            MapperContext.UserConfigurations.Add(pairing);
        }

        private static void ThrowIfSameTypes<TPairedEnum>()
        {
            if (typeof(TPairingEnum) == typeof(TPairedEnum))
            {
                throw new MappingConfigurationException(
                    "Enum pairing can only be configured between different enum types.");
            }
        }

        private static void ThrowIfEmpty<TPairedEnum>(ICollection<TPairedEnum> pairedEnumMembers)
        {
            if (pairedEnumMembers.None())
            {
                throw new MappingConfigurationException("Target enum members must be provided.");
            }
        }

        private void ThrowIfIncompatibleNumbers<TPairedEnum>(ICollection<TPairedEnum> pairedEnumMembers)
        {
            if (pairedEnumMembers.Count != 1 &&
              (_pairingEnumMembers.Length != pairedEnumMembers.Count))
            {
                throw new MappingConfigurationException(
                    $"If {pairedEnumMembers.Count} paired enum values are provided, " +
                    $"{pairedEnumMembers.Count} pairing enum values are required.");
            }
        }

        private void ThrowIfAlreadyPaired<TPairedEnum>(TPairingEnum pairingEnumValue)
        {
            if (!TryGetRelevantPairings<TPairingEnum, TPairedEnum>(out var relevantPairings))
            {
                return;
            }

            var pairingEnumValueName = pairingEnumValue.ToString();

            var confictingPairing = relevantPairings
                .FirstOrDefault(ep => ep.PairingEnumMemberName == pairingEnumValueName);

            if (confictingPairing == null)
            {
                return;
            }

            throw new MappingConfigurationException(string.Format(
                CultureInfo.InvariantCulture,
                "{0}.{1} is already paired with {2}.{3}",
                typeof(TPairingEnum).Name,
                pairingEnumValueName,
                typeof(TPairedEnum).Name,
                confictingPairing.PairedEnumMemberName));
        }

        private bool TryGetRelevantPairings<TPairing, TPaired>(out EnumMemberPair[] relevantPairings)
        {
            relevantPairings = MapperContext
                .UserConfigurations
                .GetEnumPairingsFor(typeof(TPairing), typeof(TPaired))
                .ToArray();

            return relevantPairings.Any();
        }

        private bool ValueIsNotAlreadyPaired<TPairedEnum>(TPairedEnum pairedEnumValue)
        {
            if (!TryGetRelevantPairings<TPairedEnum, TPairingEnum>(out var relevantPairings))
            {
                return true;
            }

            var pairedEnumMemberName = pairedEnumValue.ToString();

            return relevantPairings.None(pair => pair.PairingEnumMemberName == pairedEnumMemberName);
        }
    }
}