namespace AgileObjects.AgileMapper.Plans
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Members;
    using ObjectPopulation;
    using ReadableExpressions;
    using ReadableExpressions.Extensions;

    internal class MappingPlan<TSource, TTarget>
    {
        private readonly string _plan;

        public MappingPlan(MappingContext mappingContext)
        {
            var rootMappingData = mappingContext.CreateRootMapperCreationData(default(TSource), default(TTarget));

            var rootMapper = mappingContext
                .MapperContext
                .ObjectMapperFactory
                .CreateFor<TSource, TTarget>(rootMappingData);

            var planData = Expand(new MappingPlanData(rootMapper.MappingLambda, rootMappingData.MapperData));

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
            var instanceData = ((ConstantExpression)mapCall.Arguments[0]).Value;
            var targetMemberName = (string)((ConstantExpression)mapCall.Arguments[1]).Value;
            var dataSourceIndex = (int)((ConstantExpression)mapCall.Arguments[2]).Value;

            var typedExpandMethod = typeof(MappingPlan<TSource, TTarget>)
                .GetMethods(Constants.NonPublicStatic)
                .First(m => m.ContainsGenericParameters && (m.Name == "ExpandObjectMapper"))
                .MakeGenericMethod(
                    mapCall.Arguments[0].Type,
                    mapCall.Arguments[1].Type);

            var childMapperData = typedExpandMethod.Invoke(
                null,
                new[] { instanceData, targetMemberName, dataSourceIndex, mappingPlanData.MapperData });

            return (MappingPlanData)childMapperData;
        }

        // ReSharper disable once UnusedMember.Local
        private static MappingPlanData ExpandObjectMapper<TChildSource, TChildTarget>(
            MappingContext mappingContext,
            string targetMemberName,
            int dataSourceIndex,
            ObjectMapperData data)
        {
            var instanceData = new MappingInstanceData<TChildSource, TChildTarget>(
                mappingContext,
                default(TChildSource),
                default(TChildTarget));

            var mapperDataBridge = data.CreateChildMappingDataBridge(
                instanceData,
                targetMemberName,
                dataSourceIndex);

            var childMapperCreationData = mapperDataBridge.GetMapperCreationData();

            var targetType = childMapperCreationData.MapperData.TargetType;

            LambdaExpression mappingLambda;

            if (targetType == typeof(TChildTarget))
            {
                mappingLambda = GetMappingLambda<TChildSource, TChildTarget>(childMapperCreationData);
            }
            else
            {
                mappingLambda = (LambdaExpression)typeof(MappingPlan<TSource, TTarget>)
                    .GetMethod("GetMappingLambda", Constants.NonPublicStatic)
                    .MakeGenericMethod(childMapperCreationData.MapperData.SourceType, targetType)
                    .Invoke(null, new object[] { childMapperCreationData });
            }

            return new MappingPlanData(mappingLambda, childMapperCreationData.MapperData);
        }

        private static LambdaExpression GetMappingLambda<TChildSource, TChildTarget>(IObjectMapperCreationData data)
        {
            var mapper = data
                .MapperData
                .MapperContext
                .ObjectMapperFactory
                .CreateFor<TChildSource, TChildTarget>(data);

            return mapper.MappingLambda;
        }

        private static MappingPlanData ExpandElementMapper(MethodCallExpression mapCall, MappingPlanData mappingPlanData)
        {
            var typedExpandMethod = typeof(MappingPlan<TSource, TTarget>)
                .GetMethods(Constants.NonPublicStatic)
                .First(m => m.ContainsGenericParameters && (m.Name == "ExpandElementMapper"))
                .MakeGenericMethod(mapCall.Method.GetGenericArguments());

            var childMapperData = typedExpandMethod.Invoke(
                null,
                new object[] { mappingPlanData.MapperData });

            return (MappingPlanData)childMapperData;
        }

        // ReSharper disable once UnusedMember.Local
        private static MappingPlanData ExpandElementMapper<TSourceElement, TTargetElement>(ObjectMapperData data)
        {
            var elementInstanceData = new MappingInstanceData<TSourceElement, TTargetElement>(
                null,
                default(TSourceElement),
                default(TTargetElement));

            var elementMapperDataBridge = data.CreateElementMappingDataBridge(elementInstanceData);
            var elementMappingData = elementMapperDataBridge.GetMapperCreationData();

            var mappingLambda = GetMappingLambda<TSourceElement, TTargetElement>(elementMappingData);

            return new MappingPlanData(mappingLambda, elementMappingData.MapperData);
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
// Rule Set: {mappingPlanData.MapperData.RuleSet.Name}
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
