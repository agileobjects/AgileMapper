namespace AgileObjects.AgileMapper
{
    using System;
    using System.Linq.Expressions;

    internal class MappingRuleSetSettings
    {
        public bool RootHasPopulatedTarget { get; set; }

        public bool SourceElementsCouldBeNull { get; set; }

        public bool UseSingleRootMappingExpression { get; set; }

        public bool UseMemberInitialisation { get; set; }

        public bool UseTryCatch { get; set; }

        public Func<Expression, bool> GuardMemberAccesses { get; set; }

        public bool AllowEnumerableAssignment { get; set; }

        public bool AllowObjectTracking { get; set; }
    }
}