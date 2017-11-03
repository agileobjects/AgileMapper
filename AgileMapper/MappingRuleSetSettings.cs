namespace AgileObjects.AgileMapper
{
    internal class MappingRuleSetSettings
    {
        public bool RootHasPopulatedTarget { get; set; }

        public bool SourceElementsCouldBeNull { get; set; }

        public bool UseSingleRootMappingExpression { get; set; }

        public bool UseMemberInitialisation { get; set; }

        public bool UseTryCatch { get; set; }
    }
}