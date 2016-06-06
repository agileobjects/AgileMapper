namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using Members;

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
            DerivedTargetType = derivedTargetType;
        }

        public Type DerivedTargetType { get; }

        public override bool AppliesTo(IMappingData data)
            => _derivedSourceType.IsAssignableFrom(data.SourceType) && base.AppliesTo(data);
    }
}