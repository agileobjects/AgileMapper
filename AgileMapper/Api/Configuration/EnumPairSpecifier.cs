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
    /// <typeparam name="TFirstEnum">The type of the first enum being paired.</typeparam>
    public class EnumPairSpecifier<TSource, TTarget, TFirstEnum>
    {
        private readonly MappingConfigInfo _configInfo;
        private readonly TFirstEnum[] _firstEnumMembers;

        private EnumPairSpecifier(
            MappingConfigInfo configInfo,
            TFirstEnum[] firstEnumMembers)
        {
            _configInfo = configInfo;
            _firstEnumMembers = firstEnumMembers;
        }

        #region Factory Method

        internal static EnumPairSpecifier<TSource, TTarget, TFirstEnum> For(
            MappingConfigInfo configInfo,
            params TFirstEnum[] firstEnumMembers)
        {
            ThrowIfNotEnumType<TFirstEnum>();
            ThrowIfEmpty(firstEnumMembers);

            return new EnumPairSpecifier<TSource, TTarget, TFirstEnum>(configInfo, firstEnumMembers);
        }

        private static void ThrowIfNotEnumType<T>()
        {
            if (!typeof(T).IsEnum())
            {
                throw new MappingConfigurationException(
                    typeof(T).GetFriendlyName() + " is not an enum type.");
            }
        }

        private static void ThrowIfEmpty(ICollection<TFirstEnum> firstEnumMembers)
        {
            if (firstEnumMembers.None())
            {
                throw new MappingConfigurationException("Source enum members must be provided.");
            }
        }

        #endregion

        private MapperContext MapperContext => _configInfo.MapperContext;

        /// <summary>
        /// Configure this mapper to map the specified first enum member to the given <paramref name="secondEnumMember"/>.
        /// </summary>
        /// <typeparam name="TSecondEnum">The type of the second enum being paired.</typeparam>
        /// <param name="secondEnumMember">The second enum member in the pair.</param>
        /// <returns>A MappingConfigContinuation with which to configure other aspects of mapping.</returns>
        public MappingConfigContinuation<TSource, TTarget> With<TSecondEnum>(TSecondEnum secondEnumMember)
            where TSecondEnum : struct
        {
            return PairEnums(secondEnumMember);
        }

        /// <summary>
        /// Configure this mapper to map the previously-specified set of enum members to the given 
        /// <paramref name="secondEnumMembers"/>.
        /// </summary>
        /// <typeparam name="TSecondEnum">The type of the second enum being paired.</typeparam>
        /// <param name="secondEnumMembers">The second set of enum members in the pairs.</param>
        /// <returns>A MappingConfigContinuation with which to configure other aspects of mapping.</returns>
        public MappingConfigContinuation<TSource, TTarget> With<TSecondEnum>(params TSecondEnum[] secondEnumMembers)
            where TSecondEnum : struct
        {
            return PairEnums(secondEnumMembers);
        }

        private MappingConfigContinuation<TSource, TTarget> PairEnums<TSecondEnum>(params TSecondEnum[] secondEnumMembers)
        {
            ThrowIfNotEnumType<TSecondEnum>();
            ThrowIfSameTypes<TSecondEnum>();
            ThrowIfEmpty(secondEnumMembers);
            ThrowIfIncompatibleNumbers(secondEnumMembers);

            var allSecondEnumsMembersTheSame = secondEnumMembers.Length == 1;
            var firstSecondEnumMember = allSecondEnumsMembersTheSame ? secondEnumMembers[0] : default(TSecondEnum);
            var createReversePairings = _firstEnumMembers.Length == secondEnumMembers.Length;

            for (var i = 0; i < _firstEnumMembers.Length; i++)
            {
                var firstEnumMember = _firstEnumMembers[i];
                var secondEnumMember = allSecondEnumsMembersTheSame ? firstSecondEnumMember : secondEnumMembers[i];

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
              (_firstEnumMembers.Length != secondEnumMembers.Count))
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