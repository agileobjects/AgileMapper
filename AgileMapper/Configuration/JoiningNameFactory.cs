namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using Extensions;
    using Members;

    internal class JoiningNameFactory : UserConfiguredItemBase
    {
        private readonly string _separator;
        private readonly Func<string, string, IBasicMapperData, string> _joinedNameFactory;

        public JoiningNameFactory(
            string separator,
            Func<string, string, IBasicMapperData, string> joinedNameFactory,
            MappingConfigInfo configInfo)
            : base(configInfo)
        {
            _separator = separator;
            _joinedNameFactory = joinedNameFactory;
        }

        #region Factory Methods

        public static JoiningNameFactory Dotted(MapperContext mapperContext)
        {
            return new JoiningNameFactory(
                ".",
                HandleLeadingSeparator,
                MappingConfigInfo.AllRuleSetsSourceTypesAndTargetTypes(mapperContext));
        }

        public static JoiningNameFactory Flattened(MappingConfigInfo configInfo)
            => new JoiningNameFactory(string.Empty, Flatten, configInfo);

        #endregion

        public string GetJoiningName(string name, IBasicMapperData mapperData)
            => _joinedNameFactory.Invoke(_separator, name, mapperData);

        private static string HandleLeadingSeparator(string separator, string name, IBasicMapperData mapperData)
        {
            if (name.StartsWith(separator))
            {
                return mapperData.Parent.IsRoot ? name.Substring(separator.Length) : name;
            }

            return mapperData.Parent.IsRoot ? name : separator + name;
        }

        private static string Flatten(string separator, string name, IBasicMapperData mapperData)
            => name.StartsWith('.') ? name.Substring(1) : name;
    }
}