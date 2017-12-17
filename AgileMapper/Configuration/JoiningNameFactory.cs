namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Dynamic;
    using System.Linq.Expressions;
    using Api.Configuration.Dictionaries;
    using Extensions.Internal;
    using Members;
    using Members.Dictionaries;
    using ReadableExpressions.Extensions;

    internal class JoiningNameFactory : UserConfiguredItemBase
    {
        private readonly string _separator;
        private readonly Type _targetType;
        private readonly bool _isDefault;
        private readonly bool _isGlobal;
        private readonly Func<Member, IMemberMapperData, Expression> _joinedNameFactory;
        private Expression _separatorConstant;

        private JoiningNameFactory(string separator, MappingConfigInfo configInfo, bool isDefault)
            : base(configInfo)
        {
            _separator = separator;
            _targetType = configInfo.TargetType;
            _isDefault = isDefault;
            _isGlobal = _targetType == typeof(object);

            if (IsFlattened)
            {
                _joinedNameFactory = Flatten;
            }
            else
            {
                _joinedNameFactory = HandleLeadingSeparator;
            }
        }

        #region Factory Methods

        public static JoiningNameFactory UnderscoredForSourceDynamics(MapperContext mapperContext)
        {
            var sourceExpandoObject = new MappingConfigInfo(mapperContext)
                .ForAllRuleSets()
                .ForSourceType<ExpandoObject>()
                .ForAllTargetTypes();

            return ForDefault("_", sourceExpandoObject);
        }

        public static JoiningNameFactory UnderscoredForTargetDynamics(MapperContext mapperContext)
        {
            var targetExpandoObject = new MappingConfigInfo(mapperContext)
                .ForAllRuleSets()
                .ForAllSourceTypes()
                .ForTargetType<ExpandoObject>();

            return ForDefault("_", targetExpandoObject);
        }

        public static JoiningNameFactory Dotted(MapperContext mapperContext)
            => ForDefault(".", MappingConfigInfo.AllRuleSetsSourceTypesAndTargetTypes(mapperContext));

        public static JoiningNameFactory Flattened(MappingConfigInfo configInfo)
            => For(string.Empty, configInfo);

        public static JoiningNameFactory For(string separator, MappingConfigInfo configInfo)
            => new JoiningNameFactory(separator, configInfo, isDefault: false);

        public static JoiningNameFactory ForDefault(string separator, MappingConfigInfo configInfo)
            => new JoiningNameFactory(separator, configInfo, isDefault: true);

        #endregion

        public Expression Separator
            => _separatorConstant ?? (_separatorConstant = _separator.ToConstantExpression());

        public string TargetScopeDescription
            => _isGlobal ? "globally" : "for target type " + _targetType.GetFriendlyName();

        public string SeparatorDescription
            => IsFlattened ? "flattened" : "separated with '" + _separator + "'";

        private bool IsFlattened => _separator == string.Empty;

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

            if (ConfigInfo.Get<DictionaryType>() != otherConfiguredItem.ConfigInfo.Get<DictionaryType>())
            {
                return false;
            }

            return base.ConflictsWith(otherConfiguredItem);
        }

        public Expression GetJoiningName(Member member, IMemberMapperData mapperData)
            => _joinedNameFactory.Invoke(member, mapperData);

        private Expression HandleLeadingSeparator(Member member, IMemberMapperData mapperData)
        {
            var memberName = GetJoiningNamePart(member, mapperData);

            if (_separator != ".")
            {
                memberName = memberName.Replace(".", _separator);
            }

            if (memberName.StartsWith(_separator, StringComparison.Ordinal))
            {
                if (IsRootMember(member, mapperData))
                {
                    memberName = memberName.Substring(_separator.Length);
                }
            }
            else if (!IsRootMember(member, mapperData))
            {
                memberName = _separator + memberName;
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

        private static Expression Flatten(Member member, IMemberMapperData mapperData)
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