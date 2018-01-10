namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using AgileMapper.Configuration;
    using AgileMapper.Configuration.Dictionaries;
    using Members;
    using ReadableExpressions;

    internal class TargetDictionaryMappingConfigurator<TSource, TValue> :
        DictionaryMappingConfiguratorBase<TSource, Dictionary<string, TValue>>,
        ITargetDictionaryMappingInlineConfigurator<TSource, TValue>
    {
        public TargetDictionaryMappingConfigurator(MappingConfigInfo configInfo)
            : base(configInfo.Set(DictionaryType.Dictionary))
        {
        }

        #region ITargetDictionaryConfigSettings Members

        public ITargetDictionaryConfigSettings<TSource, TValue> UseFlattenedMemberNames()
        {
            SetupFlattenedTargetMemberNames();
            return this;
        }

        public ITargetDictionaryConfigSettings<TSource, TValue> UseMemberNameSeparator(string separator)
        {
            SetupMemberNameSeparator(separator);
            return this;
        }

        public ITargetDictionaryConfigSettings<TSource, TValue> UseElementKeyPattern(string pattern)
        {
            SetupElementKeyPattern(pattern);
            return this;
        }

        ITargetDictionaryMappingConfigurator<TSource, TValue> ITargetDictionaryConfigSettings<TSource, TValue>.And
            => this;

        #endregion

        public ICustomTargetDictionaryKeySpecifier<TSource, TValue> MapMember<TSourceMember>(
            Expression<Func<TSource, TSourceMember>> sourceMember)
        {
            var sourceQualifiedMember = GetSourceMemberOrThrow(sourceMember);

            return new CustomTargetDictionaryKeySpecifier<TSource, TValue>(ConfigInfo, sourceQualifiedMember);
        }

        private QualifiedMember GetSourceMemberOrThrow(LambdaExpression lambda)
        {
            var sourceMember = lambda.Body.ToSourceMember(ConfigInfo.MapperContext);

            if (sourceMember != null)
            {
                return sourceMember;
            }

            throw new MappingConfigurationException(
                $"Source member {lambda.Body.ToReadableString()} is not readable.");
        }
    }
}