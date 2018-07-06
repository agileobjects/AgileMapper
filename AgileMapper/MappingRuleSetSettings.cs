namespace AgileObjects.AgileMapper
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class MappingRuleSetSettings
    {
        public static MappingRuleSetSettings ForInMemoryMapping(bool rootHasPopulatedTarget = false)
        {
            return new MappingRuleSetSettings
            {
                RootHasPopulatedTarget = rootHasPopulatedTarget,
                RootKeysAreStaticallyCacheable = true,
                SourceElementsCouldBeNull = true,
                UseTryCatch = true,
                CheckDerivedSourceTypes = true,
                GuardAccessTo = value => true,
                ExpressionIsSupported = value => true,
                AllowObjectTracking = true,
                AllowGetMethods = true,
                AllowSetMethods = true,
                AllowIndexAccesses = true
            };
        }

        public bool RootHasPopulatedTarget { get; set; }

        public bool RootKeysAreStaticallyCacheable { get; set; }

        public bool SourceElementsCouldBeNull { get; set; }

        public bool UseSingleRootMappingExpression { get; set; }

        public bool UseMemberInitialisation { get; set; }

        public bool UseTryCatch { get; set; }

        public bool CheckDerivedSourceTypes { get; set; }

        public Func<Expression, bool> GuardAccessTo { get; set; }

        public Func<LambdaExpression, bool> ExpressionIsSupported { get; set; }

        public bool AllowEnumerableAssignment { get; set; }

        public bool AllowObjectTracking { get; set; }

        public bool AllowGetMethods { get; set; }

        public bool AllowSetMethods { get; set; }

        public bool AllowIndexAccesses { get; set; }
    }
}