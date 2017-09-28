namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using AgileMapper.Configuration;
    using Extensions;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;

    /// <summary>
    /// Provides options for specifying the enum member to which the configured enum member should be paired.
    /// </summary>
    /// <typeparam name="TFirstEnum">The type of the first enum being paired.</typeparam>
    public class EnumPairSpecifier<TFirstEnum>
    {
        private readonly MappingConfigInfo _configInfo;
        private readonly TFirstEnum[] _firstEnumMembers;

        private EnumPairSpecifier(
            MapperContext mapperContext,
            TFirstEnum[] firstEnumMembers)
        {
            _configInfo = MappingConfigInfo.AllRuleSetsSourceTypesAndTargetTypes(mapperContext);
            _firstEnumMembers = firstEnumMembers;
        }

        #region Factory Method

        internal static EnumPairSpecifier<TFirstEnum> For(
            MapperContext mapperContext,
            TFirstEnum[] firstEnumMembers)
        {
            ThrowIfNotEnumType<TFirstEnum>();
            ThrowIfEmpty(firstEnumMembers);

            return new EnumPairSpecifier<TFirstEnum>(mapperContext, firstEnumMembers);
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

        /// <summary>
        /// Configure this mapper to map the specified first enum member to the given <paramref name="secondEnumMember"/>.
        /// </summary>
        /// <typeparam name="TSecondEnum">The type of the second enum being paired.</typeparam>
        /// <param name="secondEnumMember">The second enum member in the pair.</param>
        public void With<TSecondEnum>(TSecondEnum secondEnumMember) where TSecondEnum : struct
            => With(new[] { secondEnumMember });

        /// <summary>
        /// Configure this mapper to map the previously-specified set of enum members to the given 
        /// <paramref name="secondEnumMembers"/>.
        /// </summary>
        /// <typeparam name="TSecondEnum">The type of the second enum being paired.</typeparam>
        /// <param name="secondEnumMembers">The second set of enum members in the pairs.</param>
        public void With<TSecondEnum>(params TSecondEnum[] secondEnumMembers) where TSecondEnum : struct
        {
            ThrowIfNotEnumType<TSecondEnum>();
            ThrowIfSameTypes<TSecondEnum>();
            ThrowIfEmpty(secondEnumMembers);
            ThrowIfDifferingNumbers(secondEnumMembers);

            for (var i = 0; i < _firstEnumMembers.Length; i++)
            {
                var firstEnumMember = _firstEnumMembers[i];
                var secondEnumMember = secondEnumMembers[i];

                ThrowIfAlreadyPaired(firstEnumMember, secondEnumMember);

                var firstToSecondPairing = EnumMemberPair.For(firstEnumMember, secondEnumMember);
                var secondToFirstPairing = EnumMemberPair.For(secondEnumMember, firstEnumMember);

                _configInfo.MapperContext.ValueConverters.Add(firstToSecondPairing.ValueConverter);
                _configInfo.MapperContext.ValueConverters.Add(secondToFirstPairing.ValueConverter);

                _configInfo.MapperContext.UserConfigurations.Add(firstToSecondPairing);
                _configInfo.MapperContext.UserConfigurations.Add(secondToFirstPairing);
            }
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

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private void ThrowIfDifferingNumbers<TSecondEnum>(ICollection<TSecondEnum> secondEnumMembers)
        {
            if (_firstEnumMembers.Length != secondEnumMembers.Count)
            {
                throw new MappingConfigurationException(
                    "The same number of first and second enum values must be provided.");
            }
        }

        private void ThrowIfAlreadyPaired<TSecondEnum>(TFirstEnum firstEnumMember, TSecondEnum secondEnumMember)
        {
            var firstEnumMemberName = firstEnumMember.ToString();
            var secondEnumMemberName = secondEnumMember.ToString();

            var relevantPairings = _configInfo
                .MapperContext
                .UserConfigurations
                .GetEnumPairingsFor(typeof(TFirstEnum), typeof(TSecondEnum))
                .ToArray();

            if (relevantPairings.None())
            {
                return;
            }

            var confictingPairing = relevantPairings
                .FirstOrDefault(ep => ep.FirstEnumMemberName == firstEnumMemberName);

            if (confictingPairing != null)
            {
                throw new MappingConfigurationException(string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}.{1} is already paired with {2}.{3}",
                    typeof(TFirstEnum).Name,
                    firstEnumMemberName,
                    typeof(TSecondEnum).Name,
                    confictingPairing.SecondEnumMemberName));
            }

            confictingPairing = relevantPairings
                .FirstOrDefault(ep => ep.SecondEnumMemberName == secondEnumMemberName);

            if (confictingPairing != null)
            {
                throw new MappingConfigurationException(string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}.{1} is already paired with {2}.{3}",
                    typeof(TSecondEnum).Name,
                    secondEnumMemberName,
                    typeof(TFirstEnum).Name,
                    confictingPairing.FirstEnumMemberName));
            }
        }
    }
}