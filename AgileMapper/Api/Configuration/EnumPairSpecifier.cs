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
            TFirstEnum[] firstEnumMembers)
        {
            ThrowIfNotEnumType<TFirstEnum>();
            ThrowIfEmpty(firstEnumMembers);

            return new EnumPairSpecifier<TSource, TTarget, TFirstEnum>(configInfo, firstEnumMembers);
        }

        private MapperContext MapperContext => _configInfo.MapperContext;

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
        /// <returns>An IMappingConfigContinuation with which to configure other aspects of mapping.</returns>
        public IMappingConfigContinuation<TSource, TTarget> With<TSecondEnum>(TSecondEnum secondEnumMember)
            where TSecondEnum : struct
            => With(new[] { secondEnumMember });

        /// <summary>
        /// Configure this mapper to map the previously-specified set of enum members to the given 
        /// <paramref name="secondEnumMembers"/>.
        /// </summary>
        /// <typeparam name="TSecondEnum">The type of the second enum being paired.</typeparam>
        /// <param name="secondEnumMembers">The second set of enum members in the pairs.</param>
        /// <returns>An IMappingConfigContinuation with which to configure other aspects of mapping.</returns>
        public IMappingConfigContinuation<TSource, TTarget> With<TSecondEnum>(params TSecondEnum[] secondEnumMembers)
            where TSecondEnum : struct
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

                MapperContext.ValueConverters.Add(firstToSecondPairing.ValueConverter);
                MapperContext.ValueConverters.Add(secondToFirstPairing.ValueConverter);

                MapperContext.UserConfigurations.Add(firstToSecondPairing);
                MapperContext.UserConfigurations.Add(secondToFirstPairing);
            }

            return new MappingConfigContinuation<TSource, TTarget>(_configInfo);
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

            var relevantPairings = MapperContext
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