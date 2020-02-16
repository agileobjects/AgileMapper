namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Globalization;
#if NET35
    using Microsoft.Scripting.Ast;
#endif
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;

    internal class DerivedTypePair : UserConfiguredItemBase, IComparable<DerivedTypePair>
    {
        private readonly bool _isInterfacePairing;

        public DerivedTypePair(
            MappingConfigInfo configInfo,
            Type derivedSourceType,
            Type derivedTargetType)
            : base(configInfo)
        {
            IsImplementationPairing = configInfo.TargetType.IsAbstract();
            _isInterfacePairing = IsImplementationPairing && configInfo.TargetType.IsInterface();
            DerivedSourceType = derivedSourceType;
            DerivedTargetType = derivedTargetType;
        }

        #region Factory Method

        public static DerivedTypePair For<TDerivedSource, TTarget, TDerivedTarget>(MappingConfigInfo configInfo)
        {
            ThrowIfInvalidTargetType<TTarget, TDerivedTarget>();
            ThrowIfPairingIsUnnecessary<TDerivedSource, TDerivedTarget>(configInfo);

            return new DerivedTypePair(configInfo, typeof(TDerivedSource), typeof(TDerivedTarget));
        }

        private static void ThrowIfInvalidTargetType<TTarget, TDerivedTarget>()
        {
            if (typeof(TTarget) == typeof(TDerivedTarget))
            {
                throw new MappingConfigurationException("A derived target type must be specified.");
            }
        }

        private static void ThrowIfPairingIsUnnecessary<TDerivedSource, TDerivedTarget>(MappingConfigInfo configInfo)
        {
            var memberContext = configInfo
                .Copy()
                .ForSourceType<TDerivedSource>()
                .ToMemberContext();

            var matchingAutoTypePairing = memberContext
                .GetDerivedTypePairs()
                .FirstOrDefault(tp =>
                    !tp.HasConfiguredCondition &&
                    (tp.DerivedSourceType == typeof(TDerivedSource)) &&
                    (tp.DerivedTargetType == typeof(TDerivedTarget)));

            if (matchingAutoTypePairing != null)
            {
                throw new MappingConfigurationException(string.Format(
                    CultureInfo.InvariantCulture,
                    "{0} is automatically mapped to {1} when mapping {2} to {3} and does not need to be configured.",
                    matchingAutoTypePairing.DerivedSourceType.GetFriendlyName(),
                    matchingAutoTypePairing.DerivedTargetType.GetFriendlyName(),
                    configInfo.SourceType.GetFriendlyName(),
                    configInfo.TargetType.GetFriendlyName()));
            }
        }

        #endregion

        public bool IsImplementationPairing { get; }

        public Type DerivedSourceType { get; }

        public Type DerivedTargetType { get; }

        public override bool AppliesTo(IQualifiedMemberContext context)
        {
            if (!base.AppliesTo(context))
            {
                return false;
            }

            if (context.SourceType.IsAssignableTo(DerivedSourceType))
            {
                return true;
            }

            return _isInterfacePairing &&
                    context.SourceType.IsAssignableTo(SourceType) &&
                    context.TargetType.IsAssignableTo(TargetType);
        }

        int IComparable<DerivedTypePair>.CompareTo(DerivedTypePair other)
        {
            var targetTypeX = DerivedTargetType;
            var targetTypeY = other.DerivedTargetType;

            if (targetTypeX == targetTypeY)
            {
                return 0;
            }

            if (targetTypeX.IsAssignableTo(targetTypeY))
            {
                return -1;
            }

            return 1;
        }

        #region ToString
#if DEBUG
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            var rootSourceType = SourceTypeName;
            var rootTargetType = TargetTypeName;
            var derivedSourceType = DerivedSourceType.GetFriendlyName();
            var derivedTargetType = DerivedTargetType.GetFriendlyName();

            return $"{rootSourceType} -> {rootTargetType} > {derivedSourceType} -> {derivedTargetType}";
        }
#endif
        #endregion
    }
}