#if FEATURE_DYNAMIC
namespace AgileObjects.AgileMapper.Api.Configuration.Dynamics
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using AgileMapper.Configuration;
    using Dictionaries;
    using Members;
    using ReadableExpressions;

    internal class TargetDynamicMappingConfigurator<TSource> :
        DictionaryMappingConfiguratorBase<TSource, IDictionary<string, object>>,
        ITargetDynamicMappingInlineConfigurator<TSource>
    {
        public TargetDynamicMappingConfigurator(MappingConfigInfo configInfo)
            : base(configInfo.ForTargetExpandoObject())
        {
        }

        #region ITargetDynamicConfigSettings Members

        public ITargetDynamicConfigSettings<TSource> UseFlattenedMemberNames()
        {
            SetupFlattenedTargetMemberNames();
            return this;
        }

        public ITargetDynamicConfigSettings<TSource> UseMemberNameSeparator(string separator)
        {
            SetupMemberNameSeparator(separator);
            return this;
        }

        public ITargetDynamicConfigSettings<TSource> UseElementKeyPattern(string pattern)
        {
            SetupElementKeyPattern(pattern);
            return this;
        }

        ITargetDynamicMappingConfigurator<TSource> ITargetDynamicConfigSettings<TSource>.And => this;

        #endregion

        public ICustomTargetDynamicMemberNameSpecifier<TSource> MapMember<TSourceMember>(
            Expression<Func<TSource, TSourceMember>> sourceMember)
        {
            var sourceQualifiedMember = GetSourceMemberOrThrow(sourceMember);

            return new CustomTargetDictionaryKeySpecifier<TSource, object>(ConfigInfo, sourceQualifiedMember);
        }

        private QualifiedMember GetSourceMemberOrThrow(LambdaExpression lambda)
        {
            var sourceMember = lambda.Body.ToSourceMember(MapperContext);

            if (sourceMember != null)
            {
                return sourceMember;
            }

            throw new MappingConfigurationException(
                $"Source member {lambda.Body.ToReadableString()} is not readable.");
        }
    }
}
#endif