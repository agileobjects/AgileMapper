namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;

    internal class DerivedTypePair : UserConfiguredItemBase
    {
        public DerivedTypePair(
            MappingConfigInfo configInfo,
            Type mappingTargetType,
            Type derivedSourceType,
            Type derivedTargetType)
            : base(configInfo, mappingTargetType)
        {
            DerivedTargetType = derivedTargetType;
        }

        public Type DerivedTargetType { get; }
    }
}