namespace AgileObjects.AgileMapper.Buildable
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using BuildableExpressions.SourceCode;
    using Extensions;
    using NetStandardPolyfills;
    using static System.Linq.Expressions.Expression;
    using static BuildableMapperConstants;

    internal class RepeatMappingCallReplacer : ExpressionVisitor
    {
        private readonly IDictionary<Expression, Expression> _repeatMappingCallReplacements;

        private RepeatMappingCallReplacer(
            IDictionary<Expression, Expression> repeatMappingCallReplacements)
        {
            _repeatMappingCallReplacements = repeatMappingCallReplacements;
        }

        public static void Replace(BuildableMapperGroup mapperGroup)
        {
            var mapMethodGroups = mapperGroup
                .MappingMethodsByPlan.Values
                .Filter(mapMethods => mapMethods.Count > 1)
                .ToList();

            if (mapMethodGroups.Count == 0)
            {
                return;
            }

            foreach (var mapMethodGroup in mapMethodGroups)
            {
                var repeatMappingCallReplacements = MappingCallFinder
                    .GetRepeatMappingCallReplacements(mapperGroup, mapMethodGroup);

                var replacer = new RepeatMappingCallReplacer(repeatMappingCallReplacements);

                foreach (var mapMethod in mapMethodGroup)
                {
                    replacer.ReplaceIn(mapMethod);
                }
            }
        }

        private void ReplaceIn(MethodExpressionBase mapMethod)
        {
            mapMethod.Update(VisitAndConvert(
                mapMethod.Body,
                nameof(RepeatMappingCallReplacer)));
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCall)
        {
            return _repeatMappingCallReplacements.TryGetValue(methodCall, out var replacement)
                ? replacement
                : methodCall;
        }

        #region Helper Classes

        private class MappingCallFinder : ExpressionVisitor
        {
            private readonly BuildableMapperGroup _mapperGroup;
            private readonly IDictionary<MappingTypePair, MethodExpression> _mapMethodsByMappingTypes;
            private readonly IDictionary<Expression, Expression> _mapRepeatedCallsByMappingTypes;

            private MappingCallFinder(
                BuildableMapperGroup mapperGroup,
                IEnumerable<MethodExpression> mapMethods)
            {
                _mapperGroup = mapperGroup;

                _mapRepeatedCallsByMappingTypes = new Dictionary<Expression, Expression>();

                _mapMethodsByMappingTypes = mapMethods
                    .Skip(1)
                    .ToDictionary(m => new MappingTypePair(m), MappingTypePair.Comparer);
            }

            public static IDictionary<Expression, Expression> GetRepeatMappingCallReplacements(
                BuildableMapperGroup mapperGroup,
                ICollection<MethodExpression> mapMethods)
            {
                var finder = new MappingCallFinder(mapperGroup, mapMethods);

                foreach (var mapMethod in mapMethods)
                {
                    finder.Visit(mapMethod.Body);
                }

                return finder._mapRepeatedCallsByMappingTypes;
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCall)
            {
                if (methodCall.Method.Name != MapRepeated)
                {
                    return methodCall;
                }

                var sourceObject = methodCall.Arguments[0];
                var targetObject = methodCall.Arguments[1];
                var typePair = new MappingTypePair(sourceObject.Type, targetObject.Type);

                var mapRepeatedMethod = _mapMethodsByMappingTypes[typePair];

                var createChildMappingDataMethod = _mapperGroup
                    .CreateChildMappingDataMethod
                    .MakeGenericMethod(typePair.SourceType, typePair.TargetType);

                var createMappingDataCall = Call(
                    createChildMappingDataMethod,
                    sourceObject,
                    targetObject,
                    methodCall.Object!);

                _mapRepeatedCallsByMappingTypes.Add(
                    methodCall,
                    Call(mapRepeatedMethod.MethodInfo, createMappingDataCall));

                return methodCall;
            }
        }

        private class MappingTypePair
        {
            public static readonly IEqualityComparer<MappingTypePair> Comparer =
                new MappingTypePairComparer();

            public MappingTypePair(MethodExpressionBase mapMethod)
            {
                var mappingTypes = mapMethod
                    .Parameters[0].Type
                    .GetGenericTypeArguments();

                SourceType = mappingTypes[0];
                TargetType = mappingTypes[1];
            }

            public MappingTypePair(Type sourceType, Type targetType)
            {
                SourceType = sourceType;
                TargetType = targetType;
            }

            public Type SourceType { get; }

            public Type TargetType { get; }

            private class MappingTypePairComparer : IEqualityComparer<MappingTypePair>
            {
                public bool Equals(MappingTypePair x, MappingTypePair y)
                {
                    // ReSharper disable PossibleNullReferenceException
                    return x.SourceType == y.SourceType &&
                           x.TargetType == y.TargetType;
                    // ReSharper restore PossibleNullReferenceException
                }

                public int GetHashCode(MappingTypePair obj) => 0;
            }
        }

        #endregion
    }
}