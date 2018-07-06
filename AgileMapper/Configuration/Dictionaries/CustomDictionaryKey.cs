namespace AgileObjects.AgileMapper.Configuration.Dictionaries
{
    using System;
    using System.Dynamic;
    using DataSources;
    using Extensions.Internal;
    using Members;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class CustomDictionaryKey : UserConfiguredItemBase
    {
        private CustomDictionaryKey(
            string key,
            QualifiedMember sourceMember,
            MappingConfigInfo configInfo)
            : base(configInfo)
        {
            Key = key;
            SourceMember = sourceMember;
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

        public QualifiedMember SourceMember { get; }

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

            if (IsPartOfExpandoObjectMapping(mapperData) !=
               (applicableDictionaryType == DictionaryType.Expando))
            {
                return false;
            }

            if (SourceMember == null)
            {
                return true;
            }

            var targetMember = GetTargetMember(member, mapperData);

            if (targetMember.MemberChain.Length < SourceMember.MemberChain.Length)
            {
                return false;
            }

            var targetMemberChainIndex = targetMember.MemberChain.Length;

            for (var i = SourceMember.MemberChain.Length - 1; i > 0; --i)
            {
                if (!SourceMember.MemberChain[i].Equals(targetMember.MemberChain[--targetMemberChainIndex]))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsPartOfExpandoObjectMapping(IMemberMapperData mapperData)
        {
            while (mapperData != null)
            {
                if ((mapperData.SourceMember.GetFriendlyTypeName() == nameof(ExpandoObject)) ||
                    (mapperData.TargetMember.GetFriendlyTypeName() == nameof(ExpandoObject)))
                {
                    return true;
                }

                mapperData = mapperData.Parent;
            }

            return false;
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