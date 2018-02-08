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

    internal class EnumPairSpecifier<TSource, TTarget, TFirstEnum> :
        IMappingEnumPairSpecifier<TSource, TTarget>,
        IProjectionEnumPairSpecifier<TSource, TTarget>
    {
        private readonly MappingConfigInfo _configInfo;
        private readonly TFirstEnum[] _pairingEnumMembers;

        private EnumPairSpecifier(
            MappingConfigInfo configInfo,
            TFirstEnum[] pairingEnumMembers)
        {
            _configInfo = configInfo;
            _pairingEnumMembers = pairingEnumMembers;
        }

        #region Factory Method

        public static EnumPairSpecifier<TSource, TTarget, TFirstEnum> For(
            MappingConfigInfo configInfo,
            params TFirstEnum[] pairingEnumMembers)
        {
            ThrowIfNotEnumType<TFirstEnum>();
            ThrowIfEmpty(pairingEnumMembers);

            return new EnumPairSpecifier<TSource, TTarget, TFirstEnum>(configInfo, pairingEnumMembers);
        }

        private static void ThrowIfNotEnumType<T>()
        {
            if (!typeof(T).IsEnum())
            {
                throw new MappingConfigurationException(
                    typeof(T).GetFriendlyName() + " is not an enum type.");
            }
        }

        private static void ThrowIfEmpty(ICollection<TFirstEnum> pairingEnumMembers)
        {
            if (pairingEnumMembers.None())
            {
                throw new MappingConfigurationException("Pairing enum members must be provided.");
            }
        }

        #endregion

        private MapperContext MapperContext => _configInfo.MapperContext;

        public IMappingConfigContinuation<TSource, TTarget> With<TSecondEnum>(params TSecondEnum[] pairedEnumMembers)
            where TSecondEnum : struct
        {
            return PairEnums(pairedEnumMembers);
        }

        IProjectionConfigContinuation<TSource, TTarget> IProjectionEnumPairSpecifier<TSource, TTarget>.With<TSecondEnum>(
            params TSecondEnum[] pairedEnumMembers)
        {
            return PairEnums(pairedEnumMembers);
        }

        private MappingConfigContinuation<TSource, TTarget> PairEnums<TSecondEnum>(params TSecondEnum[] pairedEnumMembers)
        {
            ThrowIfNotEnumType<TSecondEnum>();
            ThrowIfSameTypes<TSecondEnum>();
            ThrowIfEmpty(pairedEnumMembers);
            ThrowIfIncompatibleNumbers(pairedEnumMembers);

            var hasSinglePairedEnumValue = pairedEnumMembers.Length == 1;
            var firstPairedEnumMember = hasSinglePairedEnumValue ? pairedEnumMembers[0] : default(TSecondEnum);
            var createReversePairings = _pairingEnumMembers.Length == pairedEnumMembers.Length;

            for (var i = 0; i < _pairingEnumMembers.Length; i++)
            {
                var firstEnumMember = _pairingEnumMembers[i];
                var secondEnumMember = hasSinglePairedEnumValue ? firstPairedEnumMember : pairedEnumMembers[i];

                ThrowIfAlreadyPaired<TSecondEnum>(firstEnumMember);

                ConfigureEnumPair(firstEnumMember, secondEnumMember);

                if (createReversePairings)
                {
                    ConfigureEnumPair(secondEnumMember, firstEnumMember);
                }
            }

            return new MappingConfigContinuation<TSource, TTarget>(_configInfo);
        }

        private void ConfigureEnumPair<TPairing, TPaired>(TPairing pairingEnumMember, TPaired pairedEnumMember)
        {
            var pairing = EnumMemberPair.For(pairingEnumMember, pairedEnumMember);

            MapperContext.ValueConverters.Add(pairing.ValueConverter);
            MapperContext.UserConfigurations.Add(pairing);
        }

        private static void ThrowIfSameTypes<TSecondEnum>()
        {
            if (typeof(TFirstEnum) == typeof(TSecondEnum))
            {
                throw new MappingConfigurationException(
                    "Enum pairing can only be configured between different enum types.");
            }
        }

        private static void ThrowIfEmpty<TSecondEnum>(ICollection<TSecondEnum> secondEnumMembers)
        {
            if (secondEnumMembers.None())
            {
                throw new MappingConfigurationException("Target enum members must be provided.");
            }
        }

        private void ThrowIfIncompatibleNumbers<TSecondEnum>(ICollection<TSecondEnum> secondEnumMembers)
        {
            if (secondEnumMembers.Count != 1 &&
              (_pairingEnumMembers.Length != secondEnumMembers.Count))
            {
                throw new MappingConfigurationException(
                    $"If {secondEnumMembers.Count} paired enum values are provided, " +
                    $"{secondEnumMembers.Count} pairing enum values are required.");
            }
        }

        private void ThrowIfAlreadyPaired<TSecondEnum>(TFirstEnum firstEnumMember)
        {
            var relevantPairings = MapperContext
                .UserConfigurations
                .GetEnumPairingsFor(typeof(TFirstEnum), typeof(TSecondEnum))
                .ToArray();

            if (relevantPairings.None())
            {
                return;
            }

            var firstEnumMemberName = firstEnumMember.ToString();

            var confictingPairing = relevantPairings
                .FirstOrDefault(ep => ep.FirstEnumMemberName == firstEnumMemberName);

            if (confictingPairing == null)
            {
                return;
            }

            throw new MappingConfigurationException(string.Format(
                CultureInfo.InvariantCulture,
                "{0}.{1} is already paired with {2}.{3}",
                typeof(TFirstEnum).Name,
                firstEnumMemberName,
                typeof(TSecondEnum).Name,
                confictingPairing.SecondEnumMemberName));
        }
    }
}