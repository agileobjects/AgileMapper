namespace AgileObjects.AgileMapper.DataSources
{
    using System;
    using Api.Configuration;
    using Members;
    using ObjectPopulation;

    internal class ConfigurationContext : IConfigurationContext
    {
        private readonly IObjectMappingContext _omc;
        private IConfigurationContext _parent;

        public ConfigurationContext(
            QualifiedMember targetMember,
            IObjectMappingContext omc)
        {
            TargetMember = targetMember;
            _omc = omc;
        }

        public IConfigurationContext Parent
        {
            get
            {
                if (_omc.Parent == null)
                {
                    return null;
                }

                return _parent ?? (_parent = new ConfigurationContext(_omc.TargetMember, _omc.Parent));
            }
        }

        public string RuleSetName => _omc.MappingContext.RuleSet.Name;

        public QualifiedMember TargetMember { get; }

        public Type SourceObjectType => _omc.SourceObject.Type;

        public Type ExistingObjectType => _omc.ExistingObject.Type;
    }
}