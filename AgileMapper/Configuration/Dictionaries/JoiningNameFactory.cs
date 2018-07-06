namespace AgileObjects.AgileMapper.Configuration.Dictionaries
{
    using System;
    using System.Dynamic;
    using Extensions.Internal;
    using Members;
    using Members.Dictionaries;
    using ReadableExpressions.Extensions;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using static DictionaryContext;

    internal class JoiningNameFactory : DictionaryKeyPartFactoryBase
    {
        private readonly string _separator;
        private readonly bool _isDefault;
        private readonly Func<Member, IMemberMapperData, Expression> _joinedNameFactory;
        private Expression _separatorConstant;

        private JoiningNameFactory(string separator, MappingConfigInfo configInfo, bool isDefault)
            : base(configInfo)
        {
            _separator = separator;
            _isDefault = isDefault;

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
                .ForAllTargetTypes()
                .Set(DictionaryType.Expando);

            return ForDefault("_", sourceExpandoObject);
        }

        public static JoiningNameFactory UnderscoredForTargetDynamics(MapperContext mapperContext)
        {
            var targetExpandoObject = new MappingConfigInfo(mapperContext)
                .ForAllRuleSets()
                .ForAllSourceTypes()
                .ForTargetType<ExpandoObject>()
                .Set(DictionaryType.Expando);

            return ForDefault("_", targetExpandoObject);
        }

        public static JoiningNameFactory Dotted(MapperContext mapperContext)
            => ForDefault(".", MappingConfigInfo.AllRuleSetsSourceTypesAndTargetTypes);

        public static JoiningNameFactory Flattened(MappingConfigInfo configInfo)
            => For(string.Empty, configInfo);

        public static JoiningNameFactory For(string separator, MappingConfigInfo configInfo)
            => new JoiningNameFactory(separator, configInfo, isDefault: false);

        public static JoiningNameFactory ForDefault(string separator, MappingConfigInfo configInfo)
            => new JoiningNameFactory(separator, configInfo, isDefault: true);

        #endregion

        public Expression Separator
            => _separatorConstant ?? (_separatorConstant = _separator.ToConstantExpression());

        private string SeparatorDescription
            => IsFlattened ? "flattened" : "separated with '" + _separator + "'";

        private bool IsFlattened => _separator == string.Empty;

        public override bool AppliesTo(IBasicMapperData mapperData)
        {
            if (!base.AppliesTo(mapperData))
            {
                return false;
            }

            var applicableDictionarycontext = ConfigInfo.Get<DictionaryContext>();

            if (applicableDictionarycontext == All)
            {
                return true;
            }

            while (mapperData != null)
            {
                if (mapperData.TargetMember.IsDictionary)
                {
                    return false;
                }

                mapperData = mapperData.Parent;
            }

            return true;
        }

        public override bool ConflictsWith(UserConfiguredItemBase otherConfiguredItem)
        {
            var otherFactory = ((JoiningNameFactory)otherConfiguredItem);

            if (IsForAllTargetTypes != otherFactory.IsForAllTargetTypes)
            {
                return false;
            }

            var separatorsAreTheSame = _separator == otherFactory._separator;

            if (_isDefault && !separatorsAreTheSame)
            {
                return false;
            }

            if (ConfigInfo.Get<DictionaryType>() != otherFactory.ConfigInfo.Get<DictionaryType>())
            {
                return false;
            }

            var thisContext = ConfigInfo.Get<DictionaryContext>();

            if (thisContext == All)
            {
                if (separatorsAreTheSame)
                {
                    return true;
                }

                var otherContext = otherFactory.ConfigInfo.Get<DictionaryContext>();

                return otherContext == All;
            }

            return base.ConflictsWith(otherConfiguredItem);
        }

        public override string GetConflictMessage()
            => $"Member names are already configured {TargetScopeDescription} to be {SeparatorDescription}";

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

        #region ExcludeFromCodeCoverage
#if DEBUG
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public override string ToString()
        {
            var sourceType = ConfigInfo.IsForAllSourceTypes()
                ? "All sources"
                : ConfigInfo.SourceType.GetFriendlyName();

            var targetTypeName = ConfigInfo.TargetType == typeof(object)
                ? "All targets"
                : TargetTypeName;

            return $"{sourceType} -> {targetTypeName}: {SeparatorDescription}";
        }
    }
}