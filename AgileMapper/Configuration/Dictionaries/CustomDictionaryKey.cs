namespace AgileObjects.AgileMapper.Configuration.Dictionaries
{
    using System;
    using System.Dynamic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using DataSources;
    using Members;
    using Members.Extensions;
    using ReadableExpressions.Extensions;

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

        public string GetConflictMessage(ConfiguredDataSourceFactoryBase conflictingDataSource)
            => $"Configured dictionary key member {TargetMember.GetPath()} has a configured data source";

        public bool AppliesTo(Member member, IQualifiedMemberContext context)
        {
            if (!base.AppliesTo(context))
            {
                return false;
            }

            if (((ConfigInfo.SourceValueType ?? Constants.AllTypes) != Constants.AllTypes) &&
                 (context.SourceType.GetDictionaryTypes().Value != ConfigInfo.SourceValueType))
            {
                return false;
            }

            var applicableDictionaryType = ConfigInfo.Get<DictionaryType>();

            if (IsPartOfExpandoObjectMapping(context) !=
               (applicableDictionaryType == DictionaryType.Expando))
            {
                return false;
            }

            if (SourceMember == null)
            {
                return true;
            }

            var targetMember = GetTargetMember(member, context);

            if (targetMember.Depth < SourceMember.Depth)
            {
                return false;
            }

            var targetMemberChainIndex = targetMember.Depth;

            for (var i = SourceMember.Depth - 1; i > 0; --i)
            {
                if (!SourceMember.MemberChain[i].Equals(targetMember.MemberChain[--targetMemberChainIndex]))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsPartOfExpandoObjectMapping(IQualifiedMemberContext context)
        {
            while (context != null)
            {
                if ((context.SourceMember.GetTypeName() == nameof(ExpandoObject)) ||
                    (context.TargetMember.GetTypeName() == nameof(ExpandoObject)))
                {
                    return true;
                }

                context = context.Parent;
            }

            return false;
        }

        private QualifiedMember GetTargetMember(Member member, IQualifiedMemberContext context)
        {
            if (context.TargetMember.LeafMember == member)
            {
                return context.TargetMember;
            }

            var memberIndex = Array.LastIndexOf(context.TargetMember.MemberChain, member);
            var targetMemberChain = new Member[memberIndex + 1];

            for (var i = 0; i < targetMemberChain.Length; i++)
            {
                targetMemberChain[i] = context.TargetMember.MemberChain[i];
            }

            return QualifiedMember.Create(targetMemberChain, ConfigInfo.MapperContext);
        }
    }
}