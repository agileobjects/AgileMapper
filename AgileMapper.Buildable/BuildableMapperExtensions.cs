namespace AgileObjects.AgileMapper.Buildable
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using BuildableExpressions;
    using BuildableExpressions.SourceCode;
    using Extensions;
    using NetStandardPolyfills;
    using Plans;
    using ReadableExpressions.Extensions;
    using static System.Linq.Expressions.Expression;
    using static BuildableExpressions.SourceCode.MemberVisibility;

    /// <summary>
    /// Provides extension methods for building AgileMapper mapper source files.
    /// </summary>
    public static class BuildableMapperExtensions
    {
        /// <summary>
        /// Builds <see cref="SourceCodeExpression"/>s for the configured mappers in this
        /// <paramref name="mapper"/>.
        /// </summary>
        /// <param name="mapper">
        /// The <see cref="IMapper"/> for which to build <see cref="SourceCodeExpression"/>s.
        /// </param>
        /// <returns>
        /// A <see cref="SourceCodeExpression"/> for each mapper configured in this <paramref name="mapper"/>.
        /// </returns>
        public static IEnumerable<SourceCodeExpression> BuildSourceCode(
            this IMapper mapper)
        {
            yield return BuildableExpression
                .SourceCode(sourceCode =>
                {
                    sourceCode.SetNamespace("AgileObjects.AgileMapper.Buildable");

                    //var mapperClasses = new List<MapperClass>();

                    var mapperGroups = mapper
                        .GetPlansInCache()
                        .GroupBy(plan => plan.Root.SourceType)
                        .Project(grp => new
                        {
                            SourceType = grp.Key,
                            MapperName = grp.Key.GetVariableNameInPascalCase() + "Mapper",
                            Plans = grp.ToList()
                        })
                        .OrderBy(_ => _.MapperName);

                    foreach (var mapperGroup in mapperGroups)
                    {
                        var instanceMapperClass = sourceCode.AddClass(mapperGroup.MapperName, mapperClass =>
                        {
                            var sourceType = mapperGroup.SourceType;
                            var baseType = typeof(MappingExecutor<>).MakeGenericType(sourceType);
                            mapperClass.SetBaseType(baseType);

                            mapperClass.AddConstructor(ctor =>
                            {
                                var sourceParameter = ctor.AddParameter("source", sourceType);
                                
                                ctor.SetConstructorCall(
                                    baseType.GetNonPublicInstanceConstructor(sourceType),
                                    sourceParameter);

                                ctor.SetBody(Empty());
                            });

                            foreach (var plan in mapperGroup.Plans)
                            {
                                mapperClass.AddMethod("Map", doMapping =>
                                {
                                    doMapping.SetVisibility(Private);
                                    doMapping.SetStatic();
                                    doMapping.SetBody(plan.Root.Mapping);
                                });
                            }

                            var mapMethodInfosByTargetType = mapperClass.Type
                                .GetNonPublicStaticMethods("Map")
                                .ToDictionary(m => m.GetParameters()[0].ParameterType.GetGenericTypeArguments()[1]);

                            foreach (var plan in mapperGroup.Plans)
                            {
                                mapperClass.AddMethod(GetMapMethodName(plan), mapCaller =>
                                {
                                    mapCaller.SetBody(Call(
                                        mapMethodInfosByTargetType[plan.Root.TargetType]));
                                });
                            }
                        });
                    }
                });
        }

        private static string GetMapMethodName(IMappingPlan plan)
        {
            switch (plan.RuleSetName)
            {
                case "CreateNew":
                    return "ToANew";

                case "Merge":
                    return "OnTo";

                case "Overwrite":
                    return "Over";

                default:
                    throw new NotSupportedException($"Unable to map rule set '{plan.RuleSetName}'");
            }
        }
    }
}
