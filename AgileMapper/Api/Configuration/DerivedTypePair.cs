namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using Members;

    internal class DerivedTypePair : UserConfiguredItemBase
    {
        private readonly Type _derivedSourceType;

        public DerivedTypePair(
            MappingConfigInfo configInfo,
            Type mappingTargetType,
            Type derivedSourceType,
            Type derivedTargetType)
            : base(configInfo, mappingTargetType)
        {
            _derivedSourceType = derivedSourceType;
            DerivedTargetType = derivedTargetType;
        }

        public Type DerivedTargetType { get; }

        public override bool AppliesTo(IMappingData data)
            => _derivedSourceType.IsAssignableFrom(data.SourceType) && base.AppliesTo(data);
    }
}