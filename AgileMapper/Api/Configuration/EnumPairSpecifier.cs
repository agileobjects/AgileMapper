namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using AgileMapper.Configuration;
    using Extensions.Internal;
    using NetStandardPolyfills;
    using Projection;
    using ReadableExpressions.Extensions;

    internal class EnumPairSpecifier<TSource, TTarget, TPairingEnum> :
        IMappingEnumPairSpecifier<TSource, TTarget>,
        IProjectionEnumPairSpecifier<TSource, TTarget>
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

        public static EnumPairSpecifier<TSource, TTarget, TPairingEnum> For(
            MappingConfigInfo configInfo,
            params TPairingEnum[] pairingEnumMembers)
        {
            ThrowIfNotEnumType<TPairingEnum>();
            ThrowIfEmpty(pairingEnumMembers);

            return new EnumPairSpecifier<TSource, TTarget, TPairingEnum>(configInfo, pairingEnumMembers);
        }

        private static void ThrowIfNotEnumType<T>()
        {
            if (!typeof(T).IsEnum())
            {
                throw new MappingConfigurationException(
                    typeof(T).GetFriendlyName() + " is not an enum type.");
            }
        }

        private static void ThrowIfEmpty(ICollection<TPairingEnum> pairingEnumMembers)
        {
            if (pairingEnumMembers.None())
            {
                throw new MappingConfigurationException("Pairing enum members must be provided.");
            }
        }

        #endregion

        private MapperContext MapperContext => _configInfo.MapperContext;

        public IMappingConfigContinuation<TSource, TTarget> With<TPairedEnum>(params TPairedEnum[] pairedEnumMembers)
            where TPairedEnum : struct
        {
            return PairEnums(pairedEnumMembers);
        }

        IProjectionConfigContinuation<TSource, TTarget> IProjectionEnumPairSpecifier<TSource, TTarget>.With<TPairedEnum>(
            params TPairedEnum[] pairedEnumMembers)
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
                throw new MappingConfigurationException("Paired enum members must be provided.");
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