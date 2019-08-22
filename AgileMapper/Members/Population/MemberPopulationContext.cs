namespace AgileObjects.AgileMapper.Members.Population
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Configuration;
    using DataSources.Factories;
    using ObjectPopulation;

    internal class MemberPopulationContext
    {
        private IList<ConfiguredIgnoredMember> _potentialMemberIgnores;
        private ConfiguredIgnoredMember _memberIgnore;
        private DataSourceFindContext _dataSourceFindContext;

        public MemberPopulationContext(IObjectMappingData mappingData)
        {
            MappingData = mappingData;
        }

        public MappingRuleSet RuleSet => MappingContext.RuleSet;

        public MapperContext MapperContext => MappingContext.MapperContext;

        private UserConfigurationSet UserConfigurations => MapperContext.UserConfigurations;

        public IMappingContext MappingContext => MappingData.MappingContext;

        public IObjectMappingData MappingData { get; }

        private ObjectMapperData MapperData => MappingData.MapperData;

        public IMemberMapperData MemberMapperData { get; private set; }

        public QualifiedMember TargetMember => MemberMapperData.TargetMember;

        public bool AddUnsuccessfulMemberPopulations => MappingContext.AddUnsuccessfulMemberPopulations;

        private IList<ConfiguredIgnoredMember> PotentialMemberIgnores
            => _potentialMemberIgnores ??
              (_potentialMemberIgnores = UserConfigurations.GetPotentialMemberIgnores(MemberMapperData));

        public ConfiguredIgnoredMember MemberIgnore
            => _memberIgnore ?? (_memberIgnore = PotentialMemberIgnores.FindMatch(MemberMapperData));

        public bool TargetMemberIsUnconditionallyIgnored(out Expression populateCondition)
        {
            if (MemberIgnore == null)
            {
                populateCondition = null;
                return false;
            }

            populateCondition = _memberIgnore.GetConditionOrNull(MemberMapperData);
            return (populateCondition == null);
        }

        public DataSourceFindContext GetDataSourceFindContext()
        {
            var memberMappingData = MappingData.GetChildMappingData(MemberMapperData);

            if (_dataSourceFindContext == null)
            {
                _dataSourceFindContext = new DataSourceFindContext(memberMappingData);
            }

            return _dataSourceFindContext.With(memberMappingData);
        }

        public MemberPopulationContext With(QualifiedMember targetMember)
        {
            MemberMapperData = new ChildMemberMapperData(targetMember, MapperData);
            _memberIgnore = null;
            return this;
        }
    }
}