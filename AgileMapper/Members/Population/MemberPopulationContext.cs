namespace AgileObjects.AgileMapper.Members.Population
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Configuration;
    using ObjectPopulation;

    internal class MemberPopulationContext
    {
        private IList<ConfiguredIgnoredMember> _memberIgnores;
        private ConfiguredIgnoredMember _memberIgnore;

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

        public MemberPopulationContext With(QualifiedMember targetMember)
        {
            MemberMapperData = new ChildMemberMapperData(targetMember, MapperData);
            _memberIgnore = null;
            return this;
        }

        private IList<ConfiguredIgnoredMember> MemberIgnores
            => _memberIgnores ?? (_memberIgnores = UserConfigurations.GetMemberIgnoresFor(MemberMapperData));

        public ConfiguredIgnoredMember MemberIgnore
            => _memberIgnore ?? (_memberIgnore = MemberIgnores.FindMatch(MemberMapperData));

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
    }
}