#if DYNAMIC_SUPPORTED
namespace AgileObjects.AgileMapper.Api.Configuration.Dynamics
{
    using System.Collections.Generic;
    using AgileMapper.Configuration;
    using Dictionaries;

    internal class SourceDynamicMappingConfigurator<TTarget> :
        DictionaryMappingConfiguratorBase<IDictionary<string, object>, TTarget>,
        ISourceDynamicMappingConfigurator<TTarget>
    {
        public SourceDynamicMappingConfigurator(MappingConfigInfo configInfo)
            : base(configInfo)
        {
        }

        #region ISourceDynamicConfigSettings Members

        public ISourceDynamicConfigSettings<TTarget> UseMemberNameSeparator(string separator)
        {
            SetupMemberNameSeparator(separator);
            return this;
        }

        public ISourceDynamicConfigSettings<TTarget> UseElementKeyPattern(string pattern)
        {
            SetupElementKeyPattern(pattern);
            return this;
        }

        ISourceDynamicMappingConfigurator<TTarget> ISourceDynamicConfigSettings<TTarget>.And => this;

        #endregion

        public ICustomDynamicMappingTargetMemberSpecifier<TTarget> MapFullMemberName(string sourceMemberName)
            => MapFullKey<object>(sourceMemberName);

        public ICustomDynamicMappingTargetMemberSpecifier<TTarget> MapMemberName(string memberNamePart)
            => MapMemberNameKey<object>(memberNamePart);
    }
}
#endif