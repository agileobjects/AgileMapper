namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using AgileMapper.Configuration;
    using Members;
    using ReadableExpressions;

    internal class TargetDictionaryMappingConfigurator<TSource, TValue> :
        DictionaryMappingConfiguratorBase<TSource, Dictionary<string, TValue>>,
        ITargetDictionaryMappingConfigurator<TSource, TValue>
    {
        public TargetDictionaryMappingConfigurator(MappingConfigInfo configInfo)
            : base(configInfo)
        {
        }

        #region ITargetDictionaryConfigSettings Members

        public ITargetDictionaryConfigSettings<TSource, TValue> UseFlattenedMemberNames()
        {
            SetupFlattenedMemberNames();
            return this;
        }

        ITargetDictionaryMappingConfigurator<TSource, TValue> ITargetDictionaryConfigSettings<TSource, TValue>.And
            => this;

        #endregion

        public CustomTargetDictionaryKeySpecifier<TSource, TValue> MapMember<TSourceMember>(
            Expression<Func<TSource, TSourceMember>> sourceMember)
        {
            var sourceQualifiedMember = GetSourceMemberOrThrow(sourceMember);

            return new CustomTargetDictionaryKeySpecifier<TSource, TValue>(
                ConfigInfo,
                sourceQualifiedMember,
                (settings, customKey) => settings.AddMemberKey(customKey));
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