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

            var planFuncs = Expand(new MapperData(rootMapper.MappingLambda, rootOmc));

            _plan = string.Join(
                Environment.NewLine + Environment.NewLine,
                planFuncs.Select(GetDescription).Distinct());
        }

        private static IEnumerable<MapperData> Expand(MapperData mapperData)
        {
            yield return mapperData;

            var mapCalls = MapCallFinder.FindIn(mapperData.Lambda);

            foreach (var mapCall in mapCalls)
            {
                Func<MethodCallExpression, MapperData, MapperData> mappingLambdaFactory;

                if (IsObjectMemberMapping(mapCall))
                {
                    mappingLambdaFactory = ExpandObjectMapper;
                }
                else
                {
                    mappingLambdaFactory = ExpandElementMapper;
                }

                var nestedMappingFuncs = Expand(mappingLambdaFactory.Invoke(mapCall, mapperData));

                foreach (var nestedMappingFunc in nestedMappingFuncs)
                {
                    yield return nestedMappingFunc;
                }
            }
        }

        private static bool IsObjectMemberMapping(MethodCallExpression mapCall) => mapCall.Arguments.Count == 4;

        private static MapperData ExpandObjectMapper(MethodCallExpression mapCall, MapperData mapperData)
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
                new object[] { targetMemberName, dataSourceIndex, mapperData.ObjectMappingContext });

            return (MapperData)childMapperData;
        }

        // ReSharper disable once UnusedMember.Local
        private static MapperData ExpandObjectMapper<TChildSource, TChildTarget>(
            string targetMemberName,
            int dataSourceIndex,
            IObjectMappingContext omc)
        {
            var mappingCommand = omc.CreateChildMappingCommand(
                default(TChildSource),
                default(TChildTarget),
                targetMemberName,
                dataSourceIndex);

            var childOmc = mappingCommand.ToOmc();

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

            return new MapperData(mappingLambda, childOmc);
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

        private static MapperData ExpandElementMapper(MethodCallExpression mapCall, MapperData mapperData)
        {
            var typedExpandMethod = typeof(MappingPlan<TSource, TTarget>)
                .GetMethods(Constants.NonPublicStatic)
                .First(m => m.ContainsGenericParameters && (m.Name == "ExpandElementMapper"))
                .MakeGenericMethod(
                    mapCall.Arguments.ElementAt(0).Type,
                    mapCall.Arguments.ElementAt(1).Type);

            var childMapperData = typedExpandMethod.Invoke(
                null,
                new object[] { mapperData.ObjectMappingContext });

            return (MapperData)childMapperData;
        }

        // ReSharper disable once UnusedMember.Local
        private static MapperData ExpandElementMapper<TSourceElement, TTargetElement>(IObjectMappingContext omc)
        {
            var mappingCommand = omc.CreateElementMappingCommand(
                default(TSourceElement),
                default(TTargetElement),
                enumerableIndex: 0);

            var elementOmc = mappingCommand.ToOmc();

            var mappingLambda = GetMappingLambda<TSourceElement, TTargetElement>(elementOmc);

            return new MapperData(mappingLambda, elementOmc);
        }

        private static string GetDescription(MapperData mapperData)
        {
            var mappingTypes = mapperData.Lambda.Type.GetGenericArguments();
            var sourceType = mappingTypes.ElementAt(0).GetFriendlyName();
            var targetType = mappingTypes.ElementAt(1).GetFriendlyName();

            return $@"
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
// Map {sourceType} -> {targetType}
// Rule Set: {mapperData.ObjectMappingContext.RuleSetName}
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 

{mapperData.Lambda.ToReadableString()}".TrimStart();
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

        private class MapperData
        {
            public MapperData(LambdaExpression lambda, IObjectMappingContext objectMappingContext)
            {
                Lambda = lambda;
                ObjectMappingContext = objectMappingContext;
            }

            public LambdaExpression Lambda { get; }

            public IObjectMappingContext ObjectMappingContext { get; }
        }
    }
}
