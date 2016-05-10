namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using Members;

    internal static class Callbacks
    {
        public static readonly Func<IMemberMappingContext, Expression[]> Source =
            context => new[] { context.SourceObject };

        public static readonly Func<IMemberMappingContext, Expression[]> Target =
            context => new Expression[] { context.InstanceVariable };

        public static readonly Func<IMemberMappingContext, Expression[]> SourceAndTarget =
            context => new[] { context.SourceObject, context.InstanceVariable };
    }
}