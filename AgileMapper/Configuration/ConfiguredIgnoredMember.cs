namespace AgileObjects.AgileMapper.Configuration
{
    using System.Linq.Expressions;

    internal class ObjectTrackingMode : UserConfiguredItemBase
    {
        public ObjectTrackingMode(MappingConfigInfo configInfo)
            : base(configInfo)
        {
        }

        public static ObjectTrackingMode TrackAll(MapperContext mapperContext)
            => new ObjectTrackingMode(MappingConfigInfo.AllRuleSetsSourceTypesAndTargetTypes(mapperContext));
    }

    internal class ConfiguredIgnoredMember : UserConfiguredItemBase
    {
        public ConfiguredIgnoredMember(MappingConfigInfo configInfo, LambdaExpression targetMemberLambda)
            : base(configInfo, targetMemberLambda)
        {
        }
    }
}