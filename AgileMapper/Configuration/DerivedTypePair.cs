namespace AgileObjects.AgileMapper.Configuration
{
    using System;

    internal class DerivedTypePair : UserConfiguredItemBase
    {
        private readonly Type _derivedSourceType;

        public DerivedTypePair(
            MappingConfigInfo configInfo,
            Type derivedSourceType,
            Type derivedTargetType)
            : base(configInfo)
        {
            _derivedSourceType = derivedSourceType;
            ParentTargetType = configInfo.TargetType;
            DerivedTargetType = derivedTargetType;
        }

        public Type ParentTargetType { get; }

        public Type DerivedTargetType { get; }

        public override bool AppliesTo(BasicMapperData data)
            => _derivedSourceType.IsAssignableFrom(data.SourceType) && base.AppliesTo(data);
    }
}