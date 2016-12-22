namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Linq.Expressions;
    using Extensions;
    using Members;

    internal class JoiningNameFactory : UserConfiguredItemBase
    {
        private readonly string _separator;
        private readonly Func<string, string, IBasicMapperData, Expression> _joinedNameFactory;

        private JoiningNameFactory(
            string separator,
            Func<string, string, IBasicMapperData, Expression> joinedNameFactory,
            MappingConfigInfo configInfo)
            : base(configInfo)
        {
            _separator = separator;
            _joinedNameFactory = joinedNameFactory;
        }

        #region Factory Methods

        public static JoiningNameFactory Dotted(MapperContext mapperContext)
            => For(".", MappingConfigInfo.AllRuleSetsSourceTypesAndTargetTypes(mapperContext));

        public static JoiningNameFactory For(string separator, MappingConfigInfo configInfo)
            => new JoiningNameFactory(separator, HandleLeadingSeparator, configInfo);

        public static JoiningNameFactory Flattened(MappingConfigInfo configInfo)
            => new JoiningNameFactory(string.Empty, Flatten, configInfo);

        #endregion

        public Expression GetJoiningName(string name, IMemberMapperData mapperData)
        {
            var joiningName = _joinedNameFactory.Invoke(_separator, name, mapperData);

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
            if (separator != ".")
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

        private static Expression Flatten(string separator, string name, IBasicMapperData mapperData)
            => (name.StartsWith('.') ? name.Substring(1) : name).ToConstantExpression();
    }
}