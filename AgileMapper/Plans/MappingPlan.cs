namespace AgileObjects.AgileMapper.Plans
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using ReadableExpressions;
    using ReadableExpressions.Extensions;

    internal class MappingPlan<TSource, TTarget>
    {
        private readonly List<MappingPlanData> _generatedPlanData;
        private readonly string _plan;

        public MappingPlan(MappingContext mappingContext)
        {
            _generatedPlanData = new List<MappingPlanData>();

            var rootContextData = mappingContext.CreateRootMappingContextData(default(TSource), default(TTarget));

            var rootMapper = mappingContext
                .MapperContext
                .ObjectMapperFactory
                .CreateFor<TSource, TTarget>(rootContextData);

            var rootPlanData = new MappingPlanData(
                mappingContext,
                rootMapper.MappingLambda,
                rootContextData);

            var planData = Expand(rootPlanData);

            _plan = string.Join(
                Environment.NewLine + Environment.NewLine,
                planData.Select(GetDescription));

            _generatedPlanData.Clear();
        }

        private IEnumerable<MappingPlanData> Expand(MappingPlanData planData)
        {
            if (_generatedPlanData.Contains(planData))
            {
                yield break;
            }

            _generatedPlanData.Add(planData);

            yield return planData;

            var mapCalls = MapCallFinder.FindIn(planData.Lambda);

            foreach (var mapCall in mapCalls)
            {
                Func<MethodCallExpression, MappingPlanData> mappingLambdaFactory;

                if (IsObjectMemberMapping(mapCall))
                {
                    mappingLambdaFactory = planData.GetObjectMappingPlanData;
                }
                else
                {
                    mappingLambdaFactory = planData.GetElementMappingPlanData;
                }

                var nestedMappingFuncs = Expand(mappingLambdaFactory.Invoke(mapCall));

                foreach (var nestedMappingFunc in nestedMappingFuncs)
                {
                    yield return nestedMappingFunc;
                }
            }
        }

        private static bool IsObjectMemberMapping(MethodCallExpression mapCall) => mapCall.Arguments.Count == 4;

        private static string GetDescription(MappingPlanData mappingPlanData)
        {
            var mappingTypes = mappingPlanData.Lambda.Type.GetGenericArguments();
            var sourceType = mappingTypes.ElementAt(0).GetFriendlyName();
            var targetType = mappingTypes.ElementAt(1).GetFriendlyName();

            return $@"
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
// Map {sourceType} -> {targetType}
// Rule Set: {mappingPlanData.ContextData.RuleSet.Name}
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 

{mappingPlanData.Lambda.ToReadableString()}".TrimStart();
        }

        public static implicit operator string(MappingPlan<TSource, TTarget> mappingPlan) => mappingPlan._plan;

        private class MapCallFinder : ExpressionVisitor
        {
            private readonly ParameterExpression _omcParameter;
            private readonly ICollection<MethodCallExpression> _mapCalls;

            private MapCallFinder(ParameterExpression omcParameter)
            {
                _omcParameter = omcParameter;
                _mapCalls = new List<MethodCallExpression>();
            }

            public static IEnumerable<MethodCallExpression> FindIn(LambdaExpression mappingLambda)
            {
                var finder = new MapCallFinder(mappingLambda.Parameters.First());

                finder.Visit(mappingLambda);

                return finder._mapCalls;
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCall)
            {
                if (IsMapCall(methodCall))
                {
                    _mapCalls.Add(methodCall);
                }

                return base.VisitMethodCall(methodCall);
            }

            private bool IsMapCall(MethodCallExpression methodCall)
                => (methodCall.Object == _omcParameter) && (methodCall.Method.Name == "Map");
        }
    }
}
