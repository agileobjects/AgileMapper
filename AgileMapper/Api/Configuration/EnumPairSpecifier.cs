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
        private readonly TFirstEnum[] _firstEnumMembers;

        private EnumPairSpecifier(
            MappingConfigInfo configInfo,
            TFirstEnum[] firstEnumMembers)
        {
            _configInfo = configInfo;
            _firstEnumMembers = firstEnumMembers;
        }

        #region Factory Method

        public static EnumPairSpecifier<TSource, TTarget, TFirstEnum> For(
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

        public IMappingConfigContinuation<TSource, TTarget> With<TSecondEnum>(TSecondEnum secondEnumMember)
            where TSecondEnum : struct
        {
            return PairEnums(secondEnumMember);
        }

        IProjectionConfigContinuation<TSource, TTarget> IProjectionEnumPairSpecifier<TSource, TTarget>.With<TSecondEnum>(
            TSecondEnum secondEnumMember)
        {
            return PairEnums(secondEnumMember);
        }

        public IMappingConfigContinuation<TSource, TTarget> With<TSecondEnum>(params TSecondEnum[] secondEnumMembers)
            where TSecondEnum : struct
        {
            return PairEnums(secondEnumMembers);
        }

        IProjectionConfigContinuation<TSource, TTarget> IProjectionEnumPairSpecifier<TSource, TTarget>.With<TSecondEnum>(
            params TSecondEnum[] secondEnumMembers)
        {
            return PairEnums(secondEnumMembers);
        }

        private MappingConfigContinuation<TSource, TTarget> PairEnums<TSecondEnum>(params TSecondEnum[] secondEnumMembers)
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