namespace AgileObjects.AgileMapper.Plans
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using ObjectPopulation;
    using ReadableExpressions;
    using ReadableExpressions.Extensions;

    internal class MappingPlan<TSource, TTarget>
    {
        private readonly string _plan;

        public MappingPlan(MappingContext mappingContext)
        {
            var rootOmc = mappingContext.CreateRootObjectContext(default(TSource), default(TTarget));

            var rootMapper = mappingContext
                .MapperContext
                .ObjectMapperFactory
                .CreateFor<TSource, TTarget>(rootOmc);

            var planData = Expand(new MappingPlanData(rootMapper.MappingLambda, rootOmc));

            _plan = string.Join(
                Environment.NewLine + Environment.NewLine,
                planData.Distinct().Select(GetDescription));
        }

        private static IEnumerable<MappingPlanData> Expand(MappingPlanData mappingPlanData)
        {
            yield return mappingPlanData;

            var mapCalls = MapCallFinder.FindIn(mappingPlanData.Lambda);

            foreach (var mapCall in mapCalls)
            {
                Func<MethodCallExpression, MappingPlanData, MappingPlanData> mappingLambdaFactory;

                if (IsObjectMemberMapping(mapCall))
                {
                    mappingLambdaFactory = ExpandObjectMapper;
                }
                else
                {
                    mappingLambdaFactory = ExpandElementMapper;
                }

                var nestedMappingFuncs = Expand(mappingLambdaFactory.Invoke(mapCall, mappingPlanData));

                foreach (var nestedMappingFunc in nestedMappingFuncs)
                {
                    yield return nestedMappingFunc;
                }
            }
        }

        private static bool IsObjectMemberMapping(MethodCallExpression mapCall) => mapCall.Arguments.Count == 4;

        private static MappingPlanData ExpandObjectMapper(MethodCallExpression mapCall, MappingPlanData mappingPlanData)
        {
            var targetMemberName = (string)((ConstantExpression)mapCall.Arguments[2]).Value;
            var dataSourceIndex = (int)((ConstantExpression)mapCall.Arguments[3]).Value;

            var typedExpandMethod = typeof(MappingPlan<TSource, TTarget>)
                .GetMethods(Constants.NonPublicStatic)
                .First(m => m.ContainsGenericParameters && (m.Name == "ExpandObjectMapper"))
                .MakeGenericMethod(
                    mapCall.Arguments[0].Type,
                    mapCall.Arguments[1].Type);

            var childMapperData = typedExpandMethod.Invoke(
                null,
                new object[] { targetMemberName, dataSourceIndex, mappingPlanData.Omc });

            return (MappingPlanData)childMapperData;
        }

        // ReSharper disable once UnusedMember.Local
        private static MappingPlanData ExpandObjectMapper<TChildSource, TChildTarget>(
            string targetMemberName,
            int dataSourceIndex,
            IObjectMappingContext omc)
        {
            var omcBridge = omc.CreateChildMappingContextBridge(
                default(TChildSource),
                default(TChildTarget),
                targetMemberName,
                dataSourceIndex);

            var childOmc = omcBridge.ToOmc();

            var omcTypes = childOmc.GetType().GetGenericArguments();
            var omcObjectType = omcTypes.Last();

            LambdaExpression mappingLambda;

            if (omcObjectType == typeof(TChildTarget))
            {
                mappingLambda = GetMappingLambda<TChildSource, TChildTarget>(childOmc);
            }
            else
            {
                mappingLambda = (LambdaExpression)typeof(MappingPlan<TSource, TTarget>)
                    .GetMethod("GetMappingLambda", Constants.NonPublicStatic)
                    .MakeGenericMethod(omcTypes)
                    .Invoke(null, new object[] { childOmc });
            }

            return new MappingPlanData(mappingLambda, childOmc);
        }

        private static LambdaExpression GetMappingLambda<TChildSource, TChildTarget>(
            IObjectMappingContext omc)
        {
            var mapper = omc
                .MapperContext
                .ObjectMapperFactory
                .CreateFor<TChildSource, TChildTarget>(omc);

            return mapper.MappingLambda;
        }

        private static MappingPlanData ExpandElementMapper(MethodCallExpression mapCall, MappingPlanData mappingPlanData)
        {
            var typedExpandMethod = typeof(MappingPlan<TSource, TTarget>)
                .GetMethods(Constants.NonPublicStatic)
                .First(m => m.ContainsGenericParameters && (m.Name == "ExpandElementMapper"))
                .MakeGenericMethod(
                    mapCall.Arguments.ElementAt(0).Type,
                    mapCall.Arguments.ElementAt(1).Type);

            var childMapperData = typedExpandMethod.Invoke(
                null,
                new object[] { mappingPlanData.Omc });

            return (MappingPlanData)childMapperData;
        }

        // ReSharper disable once UnusedMember.Local
        private static MappingPlanData ExpandElementMapper<TSourceElement, TTargetElement>(IObjectMappingContext omc)
        {
            var elementOmcBridge = omc.CreateElementMappingContextBridge(
                default(TSourceElement),
                default(TTargetElement),
                enumerableIndex: 0);

            var elementOmc = elementOmcBridge.ToOmc();

            var mappingLambda = GetMappingLambda<TSourceElement, TTargetElement>(elementOmc);

            return new MappingPlanData(mappingLambda, elementOmc);
        }

        private static string GetDescription(MappingPlanData mappingPlanData)
        {
            var mappingTypes = mappingPlanData.Lambda.Type.GetGenericArguments();
            var sourceType = mappingTypes.ElementAt(0).GetFriendlyName();
            var targetType = mappingTypes.ElementAt(1).GetFriendlyName();

            return $@"
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
// Map {sourceType} -> {targetType}
// Rule Set: {mappingPlanData.Omc.RuleSetName}
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
