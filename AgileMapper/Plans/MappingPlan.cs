namespace AgileObjects.AgileMapper.Plans
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
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
                .CreateFor<TSource, TTarget, TTarget>(rootOmc);

            var planFuncs = Expand(new MapperData(rootMapper.MappingLambda, rootOmc));

            _plan = string.Join(
                Environment.NewLine + Environment.NewLine,
                planFuncs.Select(GetDescription));
        }

        private static IEnumerable<MapperData> Expand(MapperData mapperData)
        {
            yield return mapperData;

            var mapCalls = MapCallFinder.FindIn(mapperData.Lambda);

            Func<MethodCallExpression, MapperData, MapperData> mappingLambdaFactory;

            foreach (var mapCall in mapCalls)
            {
                if (IsObjectMemberMapping(mapCall))
                {
                    mappingLambdaFactory = ExpandObjectMapper;
                }
                else
                {
                    mappingLambdaFactory = null;
                }

                var nestedMappingFuncs = Expand(mappingLambdaFactory.Invoke(mapCall, mapperData));

                foreach (var nestedMappingFunc in nestedMappingFuncs)
                {
                    yield return nestedMappingFunc;
                }
            }
        }

        private static bool IsObjectMemberMapping(MethodCallExpression mapCall) => mapCall.Arguments.Count == 4;

        private static readonly MethodInfo _expandObjectMapperMethod = typeof(MappingPlan<TSource, TTarget>)
            .GetMethods(Constants.NonPublicStatic)
            .First(m => m.ContainsGenericParameters && (m.Name == "ExpandObjectMapper"));

        private static MapperData ExpandObjectMapper(MethodCallExpression mapCall, MapperData mapperData)
        {
            var targetMemberName = (string)((ConstantExpression)mapCall.Arguments.ElementAt(2)).Value;
            var dataSourceIndex = (int)((ConstantExpression)mapCall.Arguments.ElementAt(3)).Value;

            var typedExpandMethod = _expandObjectMapperMethod.MakeGenericMethod(
                mapCall.Arguments.ElementAt(0).Type,
                mapperData.Lambda.Type.GetGenericArguments().ElementAt(1),
                mapCall.Arguments.ElementAt(1).Type);

            var childMapperData = typedExpandMethod.Invoke(
                null,
                new object[] { targetMemberName, dataSourceIndex, mapperData.ObjectMappingContext });

            return (MapperData)childMapperData;
        }

        // ReSharper disable once UnusedMember.Local
        private static MapperData ExpandObjectMapper<TChildSource, TChildTarget, TChildObject>(
            string targetMemberName,
            int dataSourceIndex,
            IObjectMappingContext omc)
        {
            var mappingCommand = omc.CreateChildMappingCommand(
                default(TChildSource),
                default(TChildObject),
                targetMemberName,
                dataSourceIndex);

            var childOmc = mappingCommand.ToOmc();

            var childMapper = omc
                .MapperContext
                .ObjectMapperFactory
                .CreateFor<TChildSource, TChildTarget, TChildObject>(childOmc);

            return new MapperData(childMapper.MappingLambda, childOmc);
        }

        private static string GetDescription(MapperData mapperData)
        {
            var mappingTypes = mapperData.Lambda.Type.GetGenericArguments();
            var sourceType = mappingTypes.ElementAt(0).GetFriendlyName();
            var objectType = mappingTypes.ElementAt(2).GetFriendlyName();

            return $@"
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
// Map {sourceType} -> {objectType}
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
