namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Dynamic;
    using System.Linq.Expressions;
    using Api.Configuration.Dictionaries;
    using DataSources;
    using Extensions.Internal;
    using Members;

    internal class CustomDictionaryKey : UserConfiguredItemBase
    {
        private readonly QualifiedMember _sourceMember;

        private CustomDictionaryKey(
            string key,
            QualifiedMember sourceMember,
            MappingConfigInfo configInfo)
            : base(configInfo)
        {
            Key = key;
            _sourceMember = sourceMember;
        }

        private CustomDictionaryKey(
            string key,
            LambdaExpression targetMemberLambda,
            MappingConfigInfo configInfo)
            : base(configInfo, targetMemberLambda)
        {
            Key = key;
        }

        public static CustomDictionaryKey ForSourceMember(
            string key,
            QualifiedMember sourceMember,
            MappingConfigInfo configInfo)
        {
            return new CustomDictionaryKey(key, sourceMember, configInfo);
        }

        public static CustomDictionaryKey ForTargetMember(
            string key,
            LambdaExpression targetMemberLambda,
            MappingConfigInfo configInfo)
        {
            return new CustomDictionaryKey(key, targetMemberLambda, configInfo);
        }

        public string Key { get; }

        public string GetConflictMessage(ConfiguredDataSourceFactory conflictingDataSource)
            => $"Configured dictionary key member {TargetMember.GetPath()} has a configured data source";

        public bool AppliesTo(Member member, IMemberMapperData mapperData)
        {
            if (!base.AppliesTo(mapperData))
            {
                return false;
            }

            if (((ConfigInfo.SourceValueType ?? Constants.AllTypes) != Constants.AllTypes) &&
                 (mapperData.SourceType.GetDictionaryTypes().Value != ConfigInfo.SourceValueType))
            {
                return false;
            }

            var applicableDictionaryType = ConfigInfo.Get<DictionaryType>();

            if ((applicableDictionaryType != DictionaryType.ExpandoObject) &&
                (mapperData.SourceMember.GetFriendlyTypeName() == nameof(ExpandoObject)))
            {
                return false;
            }

            if ((applicableDictionaryType == DictionaryType.ExpandoObject) &&
                (mapperData.SourceMember.GetFriendlyTypeName() != nameof(ExpandoObject)))
            {
                return false;
            }

            if (_sourceMember == null)
            {
                return true;
            }

            var targetMember = GetTargetMember(member, mapperData);

            return _sourceMember.Matches(targetMember);
        }

        private QualifiedMember GetTargetMember(Member member, IBasicMapperData mapperData)
        {
            if (mapperData.TargetMember.LeafMember == member)
            {
                return mapperData.TargetMember;
            }

            var memberIndex = Array.LastIndexOf(mapperData.TargetMember.MemberChain, member);
            var targetMemberChain = new Member[memberIndex + 1];

            for (var i = 0; i < targetMemberChain.Length; i++)
            {
                targetMemberChain[i] = mapperData.TargetMember.MemberChain[i];
            }

            return QualifiedMember.From(targetMemberChain, ConfigInfo.MapperContext);
        }
    }
}