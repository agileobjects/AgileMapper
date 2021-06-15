namespace AgileObjects.AgileMapper.Buildable
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using BuildableExpressions;
    using BuildableExpressions.SourceCode;
    using BuildableExpressions.SourceCode.Api;
    using Extensions;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;
    using static System.Linq.Expressions.Expression;
    using static BuildableExpressions.SourceCode.MemberVisibility;
    using static BuildableMapperConstants;

    internal static class BuildableMapperExtensions
    {
        private const string _toolName = "AgileObjects.AgileMapper.Buildable";

        private static readonly string _version = typeof(BuildableMapperExtensions)
            .Assembly
            .GetName()
            .Version
            .ToString(fieldCount: 4);

        public static IEnumerable<SourceCodeExpression> GetPlanSourceCodeInCache(
            this IMapper mapper,
            string rootNamespace)
        {
            var mappersNamespace = rootNamespace + ".Mappers";

            var mapperClassGroups = mapper
                .GetPlansInCache()
                .GroupBy(plan => plan.Root.SourceType)
                .Project(grp => new BuildableMapperGroup(grp.Key, grp.AsEnumerable()))
                .OrderBy(grp => grp.MapperName)
                .ToList();

            if (mapperClassGroups.Count == 0)
            {
                yield break;
            }

            foreach (var mapperGroup in mapperClassGroups)
            {
                yield return BuildableExpression.SourceCode(sourceCode =>
                {
                    sourceCode.AddGeneratedCodeHeader();
                    sourceCode.SetNamespace(mappersNamespace);

                    mapperGroup.MapperClass = sourceCode.AddClass(mapperGroup.MapperName, mapperClass =>
                    {
                        mapperGroup.MapperInstance = mapperClass.ThisInstanceExpression;

                        mapperClass.AddGeneratedCodeAttribute();
                        mapperClass.SetBaseType(mapperGroup.MapperBaseType);

                        mapperClass.AddConstructor(ctor =>
                        {
                            ctor.SetConstructorCall(
                                mapperGroup.MapperBaseTypeConstructor,
                                ctor.AddParameter(mapperGroup.SourceType, "source"));

                            ctor.SetBody(Empty());
                        });

                        foreach (var planAndMappingMethods in mapperGroup.MappingMethodsByPlan)
                        {
                            var plan = planAndMappingMethods.Key;
                            var mappingMethods = planAndMappingMethods.Value;

                            foreach (var repeatPlan in plan.Skip(1))
                            {
                                mappingMethods.Add(mapperClass.AddMethod(MapRepeated, doMapping =>
                                {
                                    doMapping.SetVisibility(Private);
                                    doMapping.SetStatic();
                                    doMapping.SetBody(repeatPlan.Mapping);
                                }));
                            }

                            mappingMethods.Insert(0, mapperClass.AddMethod(plan.RuleSetName, doMapping =>
                            {
                                doMapping.SetVisibility(Private);
                                doMapping.SetStatic();
                                doMapping.SetBody(plan.Root.Mapping);
                            }));
                        }

                        RepeatMappingCallReplacer.Replace(mapperGroup);

                        var allRuleSetMapMethodInfos = mapperGroup
                            .MappingMethodsByPlan.Values
                            .SelectMany(methods => methods)
                            .Filter(method => method.Name != MapRepeated)
                            .Project(method => new MapMethodInfo(mapperGroup, method))
                            .GroupBy(m => m.RuleSetName)
                            .Select(methodGroup => methodGroup.ToList());

                        foreach (var ruleSetMapMethodInfos in allRuleSetMapMethodInfos)
                        {
                            AddMapMethodsFor(mapperClass, ruleSetMapMethodInfos);
                        }
                    });
                });
            }

            yield return BuildableExpression.SourceCode(sourceCode =>
            {
                sourceCode.AddGeneratedCodeHeader();
                sourceCode.SetNamespace(mappersNamespace);

                sourceCode.AddClass("Mapper", staticMapperClass =>
                {
                    staticMapperClass.AddGeneratedCodeAttribute();
                    staticMapperClass.SetStatic();

                    foreach (var mapperClassGroup in mapperClassGroups)
                    {
                        var sourceType = mapperClassGroup.SourceType;
                        var mapperClass = mapperClassGroup.MapperClass;

                        staticMapperClass.AddMethod("Map", mapMethod =>
                        {
                            var sourceParameter = mapMethod
                                .AddParameter(sourceType, "source");

                            var newMapper = New(
                                mapperClass.Type.GetPublicInstanceConstructor(sourceType),
                                sourceParameter);

                            mapMethod.SetBody(newMapper);
                        });
                    }
                });
            });
        }

        private static void AddMapMethodsFor(
            IClassMemberConfigurator mapperClass,
            IList<MapMethodInfo> mapMethodInfos)
        {
            var ruleSetName = mapMethodInfos[0].RuleSetName;

            switch (ruleSetName)
            {
                case "CreateNew":
                    AddCreateNewMapMethod(mapperClass, mapMethodInfos);
                    return;

                case "Merge":
                    AddUpdateInstanceMapMethod(mapperClass, mapMethodInfos, "OnTo");
                    return;

                case "Overwrite":
                    AddUpdateInstanceMapMethod(mapperClass, mapMethodInfos, "Over");
                    return;

                default:
                    throw new NotSupportedException($"Unable to map rule set '{ruleSetName}'");
            }
        }

        private static void AddCreateNewMapMethod(
            IClassMemberConfigurator mapperClass,
            IList<MapMethodInfo> mapMethodInfos)
        {
            mapperClass.AddMethod("ToANew", mapNewMethod =>
            {
                var useTypeConstraint = UseTypeConstraint(mapMethodInfos);

                var targetGenericParameter = mapNewMethod.AddGenericParameter("TTarget", param =>
                {
                    if (useTypeConstraint)
                    {
                        param.AddTypeConstraints(mapMethodInfos[0].TargetType);
                    }
                });

                if (useTypeConstraint)
                {
                    mapNewMethod.SetBody(mapMethodInfos[0].CreateMapCall(Default));
                    return;
                }

                var targetGenericParameterType = targetGenericParameter.Type;
                var typeofTarget = BuildableExpression.TypeOf(targetGenericParameter);

                var mappingExpressions = new List<Expression>();
                var returnTarget = Label(targetGenericParameterType, "Return");

                foreach (var mapMethodInfo in mapMethodInfos)
                {
                    var targetType = mapMethodInfo.TargetType;
                    var typeofTargetType = BuildableExpression.TypeOf(targetType);

                    var typesAssignable = targetType.IsSealed()
                        ? (Expression)Equal(typeofTarget, typeofTargetType)
                        : Call(IsAssignableToMethod, typeofTarget, typeofTargetType);

                    var mapCall = mapMethodInfo.CreateMapCall(Default);
                    var mapCallResultAsObject = Convert(mapCall, typeof(object));
                    var mapCallResultAsTarget = Convert(mapCallResultAsObject, targetGenericParameterType);
                    var returnMapResult = Return(returnTarget, mapCallResultAsTarget);

                    var ifTypesMatchMap = IfThen(typesAssignable, returnMapResult);
                    mappingExpressions.Add(ifTypesMatchMap);
                }

                mappingExpressions.Add(GetThrowTargetNotSupportedException(mapMethodInfos[0], targetGenericParameter));
                mappingExpressions.Add(Label(returnTarget, Default(targetGenericParameterType)));

                mapNewMethod.SetBody(Block(mappingExpressions));
            });
        }

        private static bool UseTypeConstraint(IList<MapMethodInfo> mapMethodInfos)
        {
            if (mapMethodInfos.Count != 1)
            {
                return false;
            }

            var targetType = mapMethodInfos[0].TargetType;

            return !(targetType.IsArray || targetType.IsEnum());
        }

        private static Expression GetThrowTargetNotSupportedException(
            MapMethodInfo mapMethodInfo,
            TypeExpression targetGenericParameter)
        {
            var getErrorMessageCall = Call(
                StringConcatMethod,
                Constant(
                    $"Unable to perform a '{mapMethodInfo.RuleSetName}' mapping " +
                    $"from source type '{mapMethodInfo.SourceType.GetFriendlyName()}' " +
                     "to target type '",
                    typeof(string)),
                Call(
                    GetFriendlyNameMethod,
                    BuildableExpression.TypeOf(targetGenericParameter),
                    NullConfiguration),
                Constant("'", typeof(string)));

            return Throw(New(NotSupportedCtor, getErrorMessageCall));
        }

        private static void AddUpdateInstanceMapMethod(
            IClassMemberConfigurator mapperClass,
            IEnumerable<MapMethodInfo> mapMethodInfos,
            string apiMethodName)
        {
            foreach (var mapMethodInfo in mapMethodInfos)
            {
                mapperClass.AddMethod(apiMethodName, mapMethod =>
                {
                    mapMethod.SetBody(mapMethodInfo
                        .CreateMapCall(t => mapMethod.AddParameter(t, "target")));
                });
            }
        }

        private static void AddGeneratedCodeHeader(
            this ISourceCodeExpressionConfigurator sourceCode)
        {
            sourceCode.SetHeader(@$"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by {_toolName}.
//     Runtime Version: {_version}
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------".TrimStart());
        }

        private static void AddGeneratedCodeAttribute(
            this IAttributableExpressionConfigurator mapperClass)
        {
            mapperClass.AddAttribute(typeof(GeneratedCodeAttribute), cfg =>
            {
                cfg.SetConstructorArguments(_toolName, _version);
            });
        }

        #region Helper Members

        private class MapMethodInfo
        {
            private readonly BuildableMapperGroup _mapperGroup;
            private readonly MethodExpression _mapMethod;

            public MapMethodInfo(
                BuildableMapperGroup mapperGroup,
                MethodExpression mapMethod)
            {
                _mapperGroup = mapperGroup;
                _mapMethod = mapMethod;
                TargetType = mapMethod.GetTargetType();
            }

            public string RuleSetName => _mapMethod.Name;

            public Type SourceType => _mapperGroup.SourceType;

            public Type TargetType { get; }

            public Expression CreateMapCall(Func<Type, Expression> targetFactory)
            {
                var createMappingDataCall = Call(
                    _mapperGroup.MapperInstance,
                    _mapperGroup.CreateRootMappingDataMethod.MakeGenericMethod(TargetType),
                    targetFactory.Invoke(TargetType));

                return Call(_mapMethod.MethodInfo, createMappingDataCall);
            }
        }

        #endregion
    }
}
