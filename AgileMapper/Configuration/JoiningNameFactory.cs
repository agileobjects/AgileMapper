namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Linq.Expressions;
    using Extensions;
    using Members;

    internal class JoiningNameFactory : UserConfiguredItemBase
    {
        private readonly Func<string, string, IBasicMapperData, Expression> _joinedNameFactory;

        private JoiningNameFactory(
            string separator,
            Func<string, string, IBasicMapperData, Expression> joinedNameFactory,
            MappingConfigInfo configInfo)
            : base(configInfo)
        {
            Separator = separator;
            _joinedNameFactory = joinedNameFactory;
            TargetType = configInfo.TargetType;
            IsDefault = HasDefault(separator);
            IsFlattened = !IsDefault && (separator == string.Empty);
            IsGlobal = TargetType == typeof(object);
        }

        #region Factory Methods

        public static JoiningNameFactory Dotted(MapperContext mapperContext)
            => For(".", MappingConfigInfo.AllRuleSetsSourceTypesAndTargetTypes(mapperContext));

        public static JoiningNameFactory For(string separator, MappingConfigInfo configInfo)
            => new JoiningNameFactory(separator, HandleLeadingSeparator, configInfo);

        public static JoiningNameFactory Flattened(MappingConfigInfo configInfo)
            => new JoiningNameFactory(string.Empty, Flatten, configInfo);

        #endregion

        public Type TargetType { get; }

        public bool IsGlobal { get; }

        public bool IsDefault { get; }

        public bool IsFlattened { get; }

        public string Separator { get; }

        public Expression GetJoiningName(string name, IMemberMapperData mapperData)
        {
            var joiningName = _joinedNameFactory.Invoke(Separator, name, mapperData);

            if (mapperData.Parent.IsRoot)
            {
                return joiningName;
            }

            var condition = GetConditionOrNull(mapperData);

            if (condition == null)
            {
                return joiningName;
            }

            var dottedJoiningName = HandleLeadingSeparator(".", name, mapperData);

            return Expression.Condition(condition, joiningName, dottedJoiningName);
        }

        private static Expression HandleLeadingSeparator(string separator, string name, IBasicMapperData mapperData)
        {
            if (!HasDefault(separator))
            {
                name = name.Replace(".", separator);
            }

            if (name.StartsWith(separator))
            {
                if (mapperData.Parent.IsRoot)
                {
                    name = name.Substring(separator.Length);
                }
            }
            else if (!mapperData.Parent.IsRoot)
            {
                name = separator + name;
            }

            return name.ToConstantExpression();
        }

        private static bool HasDefault(string separator) => separator == ".";

        private static Expression Flatten(string separator, string name, IBasicMapperData mapperData)
            => (name.StartsWith('.') ? name.Substring(1) : name).ToConstantExpression();
    }
}