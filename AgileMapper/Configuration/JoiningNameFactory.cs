namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Dynamic;
    using System.Linq.Expressions;
    using Extensions.Internal;
    using Members;
    using Members.Dictionaries;
    using ReadableExpressions.Extensions;

    internal class JoiningNameFactory : UserConfiguredItemBase
    {
        private readonly string _separator;
        private readonly Func<string, Member, IMemberMapperData, Expression> _joinedNameFactory;
        private readonly Type _targetType;
        private readonly bool _isDefault;
        private readonly bool _isGlobal;
        private Expression _separatorConstant;

        private JoiningNameFactory(
            string separator,
            Func<string, Member, IMemberMapperData, Expression> joinedNameFactory,
            MappingConfigInfo configInfo)
            : base(configInfo)
        {
            _separator = separator;
            _joinedNameFactory = joinedNameFactory;
            _targetType = configInfo.TargetType;
            _isDefault = HasDefault(separator);
            _isGlobal = _targetType == typeof(object);
        }

        #region Factory Methods

        public static JoiningNameFactory UnderscoredForSourceDynamics(MapperContext mapperContext)
        {
            var sourceExpandoObject = new MappingConfigInfo(mapperContext)
                .ForAllRuleSets()
                .ForSourceType<ExpandoObject>()
                .ForAllTargetTypes();

            return For("_", sourceExpandoObject);
        }

        public static JoiningNameFactory UnderscoredForTargetDynamics(MapperContext mapperContext)
        {
            var targetExpandoObject = new MappingConfigInfo(mapperContext)
                .ForAllRuleSets()
                .ForAllSourceTypes()
                .ForTargetType<ExpandoObject>();

            return For("_", targetExpandoObject);
        }

        public static JoiningNameFactory Dotted(MapperContext mapperContext)
            => For(".", MappingConfigInfo.AllRuleSetsSourceTypesAndTargetTypes(mapperContext));

        public static JoiningNameFactory For(string separator, MappingConfigInfo configInfo)
            => new JoiningNameFactory(separator, HandleLeadingSeparator, configInfo);

        public static JoiningNameFactory Flattened(MappingConfigInfo configInfo)
            => new JoiningNameFactory(string.Empty, Flatten, configInfo);

        #endregion

        public string TargetScopeDescription
            => _isGlobal ? "globally" : "for target type " + _targetType.GetFriendlyName();

        public string SeparatorDescription
            => IsFlattened ? "flattened" : "separated with '" + _separator + "'";

        private bool IsFlattened => !_isDefault && (_separator == string.Empty);

        public override bool ConflictsWith(UserConfiguredItemBase otherConfiguredItem)
        {
            if (_isDefault)
            {
                return false;
            }

            if (_isGlobal != ((JoiningNameFactory)otherConfiguredItem)._isGlobal)
            {
                return false;
            }

            return base.ConflictsWith(otherConfiguredItem);
        }

        public Expression Separator
            => _separatorConstant ?? (_separatorConstant = _separator.ToConstantExpression());

        public Expression GetJoiningName(Member member, IMemberMapperData mapperData)
            => _joinedNameFactory.Invoke(_separator, member, mapperData);

        private static Expression HandleLeadingSeparator(string separator, Member member, IMemberMapperData mapperData)
        {
            var memberName = GetJoiningNamePart(member, mapperData);

            if (!HasDefault(separator))
            {
                memberName = memberName.Replace(".", separator);
            }

            if (memberName.StartsWith(separator, StringComparison.Ordinal))
            {
                if (IsRootMember(member, mapperData))
                {
                    memberName = memberName.Substring(separator.Length);
                }
            }
            else if (!IsRootMember(member, mapperData))
            {
                memberName = separator + memberName;
            }

            return memberName.ToConstantExpression();
        }

        private static string GetJoiningNamePart(Member member, IMemberMapperData mapperData)
        {
            var dictionarySettings = mapperData.MapperContext.UserConfigurations.Dictionaries;
            var memberName = dictionarySettings.GetMemberKeyOrNull(member, mapperData) ?? member.JoiningName;

            return memberName;
        }

        private static bool IsRootMember(Member member, IMemberMapperData mapperData)
        {
            var memberIndex = Array.IndexOf(mapperData.TargetMember.MemberChain, member, 0);

            if (memberIndex == 1)
            {
                return true;
            }

            if (mapperData.TargetMember is DictionaryTargetMember)
            {
                return mapperData.TargetMember.MemberChain[memberIndex - 1].IsDictionary;
            }

            var rootDictionaryContextIndex = mapperData.TargetMember.MemberChain.Length;
            var sourceMember = mapperData.SourceMember;

            while (mapperData.SourceMember.Type == sourceMember.Type)
            {
                --rootDictionaryContextIndex;
                mapperData = mapperData.Parent;

                if (mapperData == null)
                {
                    return false;
                }
            }

            memberIndex = memberIndex - rootDictionaryContextIndex;

            return memberIndex == 1;
        }

        private static bool HasDefault(string separator) => separator == ".";

        private static Expression Flatten(string separator, Member member, IMemberMapperData mapperData)
        {
            var memberName = GetJoiningNamePart(member, mapperData);

            if (memberName.StartsWith('.'))
            {
                memberName = memberName.Substring(1);
            }

            return memberName.ToConstantExpression();
        }
    }
}