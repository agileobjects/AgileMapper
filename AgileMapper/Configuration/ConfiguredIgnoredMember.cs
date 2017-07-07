namespace AgileObjects.AgileMapper.Configuration
{
    using System.Linq.Expressions;
    using DataSources;
    using Members;

    internal class ConfiguredIgnoredMember : UserConfiguredItemBase, IPotentialClone
    {
        public ConfiguredIgnoredMember(MappingConfigInfo configInfo, LambdaExpression targetMemberLambda)
            : base(configInfo, targetMemberLambda)
        {
        }

        private ConfiguredIgnoredMember(MappingConfigInfo configInfo, QualifiedMember targetMember)
            : base(configInfo, targetMember)
        {
        }

        public string GetConflictMessage() => $"Member {TargetMember.GetPath()} has been ignored";

        public string GetConflictMessage(ConfiguredIgnoredMember conflictingIgnoredMember)
            => $"Member {TargetMember.GetPath()} has already been ignored";

        public string GetConflictMessage(ConfiguredDataSourceFactory conflictingDataSource)
            => $"Ignored member {TargetMember.GetPath()} has a configured data source";

        #region IPotentialClone Members

        public bool IsClone { get; private set; }

        public IPotentialClone Clone()
        {
            return new ConfiguredIgnoredMember(ConfigInfo, TargetMember)
            {
                IsClone = true
            };
        }

        #endregion
    }
}