namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Members;

    internal static class Callbacks
    {
        public static readonly KeyValuePair<int, Func<IMemberMappingContext, Expression[]>> Source =
            CreatePair(1, context => new[] { context.SourceObject });

        public static readonly KeyValuePair<int, Func<IMemberMappingContext, Expression[]>> Target =
            CreatePair(1, context => new Expression[] { context.TargetVariable });

        public static readonly KeyValuePair<int, Func<IMemberMappingContext, Expression[]>> SourceAndTarget =
            CreatePair(2, context => new[] { context.SourceObject, context.TargetVariable });

        private static KeyValuePair<int, Func<IMemberMappingContext, Expression[]>> CreatePair(
            int parameterCount,
            Func<IMemberMappingContext, Expression[]> parameterReplacementsFactory)
        {
            return new KeyValuePair<int, Func<IMemberMappingContext, Expression[]>>(parameterCount, parameterReplacementsFactory);
        }
    }
}