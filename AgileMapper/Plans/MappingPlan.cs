namespace AgileObjects.AgileMapper.Plans
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using ObjectPopulation;
    using NetStandardPolyfills;

    internal class MappingPlan<TSource, TTarget>
    {
        private readonly List<MappingPlanData> _generatedPlanData;

        public MappingPlan(IMappingContext mappingContext)
        {
            _generatedPlanData = new List<MappingPlanData>();

            var rootMappingData = mappingContext
                .CreateRootMappingData(default(TSource), default(TTarget));

            var rootPlanData = new MappingPlanData(rootMappingData);

            Expand(rootPlanData);
        }

        private void Expand(MappingPlanData planData)
        {
            if (_generatedPlanData.Contains(planData))
            {
                return;
            }

            _generatedPlanData.Add(planData);

            var mapCalls = MapCallFinder.FindIn(planData.Lambda);

            foreach (var mapCall in mapCalls)
            {
                Func<MethodCallExpression, MappingPlanData> mappingLambdaFactory;

                if (IsObjectMemberMapping(mapCall))
                {
                    mappingLambdaFactory = planData.GetChildMappingPlanData;
                }
                else
                {
                    mappingLambdaFactory = planData.GetElementMappingPlanData;
                }

                Expand(mappingLambdaFactory.Invoke(mapCall));
            }
        }

        private static bool IsObjectMemberMapping(MethodCallExpression mapCall) => mapCall.Arguments.Count == 4;

        public static implicit operator string(MappingPlan<TSource, TTarget> mappingPlan)
        {
            return string.Join(
                Environment.NewLine + Environment.NewLine,
                mappingPlan._generatedPlanData.Select(pd => pd.GetDescription()));
        }

        private class MapCallFinder : ExpressionVisitor
        {
            private readonly ICollection<MethodCallExpression> _mapCalls;

            private MapCallFinder()
            {
                _mapCalls = new List<MethodCallExpression>();
            }

            public static IEnumerable<MethodCallExpression> FindIn(Expression mappingLambda)
            {
                var finder = new MapCallFinder();

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

            private static bool IsMapCall(MethodCallExpression methodCall)
            {
                return (methodCall.Method.Name == "Map") &&
                       (methodCall.Object != null) &&
                        methodCall.Object.Type.IsGenericType() &&
                       (methodCall.Object.Type.GetGenericTypeDefinition() == typeof(IObjectMappingData<,>));
            }
        }
    }
}
