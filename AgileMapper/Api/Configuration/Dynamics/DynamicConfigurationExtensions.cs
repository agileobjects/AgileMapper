#if FEATURE_DYNAMIC
namespace AgileObjects.AgileMapper.Api.Configuration.Dynamics
{
    using System.Dynamic;
    using AgileMapper.Configuration;
    using AgileMapper.Configuration.Dictionaries;
    using Dictionaries;

    internal static class DynamicConfigurationExtensions
    {
        public static MappingConfigInfo ForSourceExpandoObject(this MappingConfigInfo configInfo)
            => configInfo.ForAllRuleSetsExpandoObject().ForSourceType<ExpandoObject>();

        public static MappingConfigInfo ForTargetExpandoObject(this MappingConfigInfo configInfo)
            => configInfo.ForAllRuleSetsExpandoObject().ForTargetType<ExpandoObject>();

        private static MappingConfigInfo ForAllRuleSetsExpandoObject(this MappingConfigInfo configInfo)
            => configInfo.ForAllRuleSets().ForExpandoObject();

        public static MappingConfigInfo ForExpandoObject(this MappingConfigInfo configInfo) 
            => configInfo.Set(DictionaryType.Expando).WithMemberTypeComparers();
    }
}
#endif