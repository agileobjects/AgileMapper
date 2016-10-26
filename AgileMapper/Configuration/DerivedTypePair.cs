namespace AgileObjects.AgileMapper.Configuration
{
    using System;
#if NET_STANDARD
    using System.Reflection;
#endif
    using Members;

    internal class DerivedTypePair : UserConfiguredItemBase
    {
        private readonly Type _derivedSourceType;

        private DerivedTypePair(
            MappingConfigInfo configInfo,
            Type derivedSourceType,
            Type derivedTargetType)
            : base(configInfo)
        {
            _derivedSourceType = derivedSourceType;
            DerivedTargetType = derivedTargetType;
        }

        public static DerivedTypePair For<TDerivedSource, TTarget, TDerivedTarget>(MappingConfigInfo configInfo)
        {
            return new DerivedTypePair(
                configInfo.ForTargetType<TTarget>(),
                typeof(TDerivedSource),
                typeof(TDerivedTarget));
        }

        public Type DerivedTargetType { get; }

        public override bool AppliesTo(IBasicMapperData mapperData)
            => _derivedSourceType.IsAssignableFrom(mapperData.SourceType) && base.AppliesTo(mapperData);

        protected override bool TargetMembersMatch(IBasicMapperData mapperData) => true;
    }
}