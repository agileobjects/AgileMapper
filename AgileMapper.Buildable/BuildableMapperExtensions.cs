namespace AgileObjects.AgileMapper.Buildable
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using AgileObjects.ReadableExpressions;
    using BuildableExpressions;
    using BuildableExpressions.SourceCode;
    using BuildableExpressions.SourceCode.Api;
    using Extensions;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;
    using static System.Linq.Expressions.Expression;
    using static BuildableExpressions.SourceCode.MemberVisibility;
    using PublicTypeExtensions = ReadableExpressions.Extensions.PublicTypeExtensions;

    /// <summary>
    /// Provides extension methods for building AgileMapper mapper source files.
    /// </summary>
    public static class BuildableMapperExtensions
    {
        private static readonly Expression _doNotCreateMapper = Constant(false, typeof(bool));

        private static readonly MethodInfo _isAssignableToMethod = typeof(TypeExtensionsPolyfill)
            .GetPublicStaticMethod(nameof(TypeExtensionsPolyfill.IsAssignableTo));

        private static readonly ConstructorInfo _notSupportedCtor = typeof(NotSupportedException)
            .GetPublicInstanceConstructor(typeof(string));

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

                    var mapperClassGroups = mapper
                        .GetPlansInCache()
                        .GroupBy(plan => plan.Root.SourceType)
                        .Project(grp => new
                        {
                            SourceType = grp.Key,
                            MapperName = grp.Key.GetVariableNameInPascalCase() + "Mapper",
                            Plans = grp.ToList()
                        })
                        .OrderBy(_ => _.MapperName);

                    var mapperClassesBySourceType = new Dictionary<Type, ClassExpression>();

                    foreach (var mapperGroup in mapperClassGroups)
                    {
                        var sourceType = mapperGroup.SourceType;

                        mapperClassesBySourceType.Add(sourceType, sourceCode.AddClass(
                            mapperGroup.MapperName,
                            mapperClass =>
                            {
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
                                    mapperClass.AddMethod(plan.RuleSetName, doMapping =>
                                    {
                                        doMapping.SetVisibility(Private);
                                        doMapping.SetStatic();
                                        doMapping.SetBody(plan.Root.Mapping);
                                    });
                                }

                                var createMappingDataMethod = baseType
                                    .GetNonPublicInstanceMethod("CreateRootMappingData");

                                var allRuleSetMapMethodInfos = mapperClass.Type
                                    .GetNonPublicStaticMethods()
                                    .Project(m => new MapMethodInfo(
                                        sourceType,
                                        mapperClass,
                                        createMappingDataMethod,
                                        m))
                                    .GroupBy(m => m.RuleSetName)
                                    .Select(methodGroup => methodGroup.ToList());

                                foreach (var ruleSetMapMethodInfos in allRuleSetMapMethodInfos)
                                {
                                    AddMapMethodsFor(mapperClass, ruleSetMapMethodInfos);
                                }
                            }));
                    }

                    sourceCode.AddClass("Mapper", staticMapperClass =>
                    {
                        staticMapperClass.SetStatic();

                        foreach (var sourceTypeAndMapper in mapperClassesBySourceType)
                        {
                            var sourceType = sourceTypeAndMapper.Key;
                            var mapperClass = sourceTypeAndMapper.Value;

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
                var hasSingleTarget = mapMethodInfos.Count == 1;

                var targetGenericParameter = mapNewMethod.AddGenericParameter("TTarget", param =>
                {
                    if (hasSingleTarget)
                    {
                        param.AddTypeConstraints(mapMethodInfos[0].TargetType);
                    }
                });

                if (hasSingleTarget)
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
                    var typesAssignable = Call(_isAssignableToMethod, typeofTarget, typeofTargetType);

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

        private static Expression GetThrowTargetNotSupportedException(
            MapMethodInfo mapMethodInfo,
            TypeExpression targetGenericParameter)
        {
            var stringConcatMethod = typeof(string).GetPublicStaticMethod(
                nameof(string.Concat),
                typeof(string),
                typeof(string),
                typeof(string));

            var nullConfiguration = Default(typeof(Func<ITranslationSettings, ITranslationSettings>));

            var getFriendlyNameMethod = typeof(PublicTypeExtensions).GetPublicStaticMethod(
                nameof(PublicTypeExtensions.GetFriendlyName),
                typeof(Type),
                nullConfiguration.Type);

            var getErrorMessageCall = Call(
                stringConcatMethod,
                Constant(
                    $"Unable to perform a '{mapMethodInfo.RuleSetName}' mapping " +
                    $"from source type '{mapMethodInfo.SourceType.GetFriendlyName()}' " +
                     "to target type '",
                    typeof(string)),
                Call(
                    getFriendlyNameMethod,
                    BuildableExpression.TypeOf(targetGenericParameter),
                    nullConfiguration),
                Constant("'", typeof(string)));

            return Throw(New(_notSupportedCtor, getErrorMessageCall));
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

        #region Helper Members

        private class MapMethodInfo
        {
            private readonly IClassExpressionConfigurator _mapperClass;
            private readonly MethodInfo _createMappingDataMethod;
            private readonly MethodInfo _mapMethod;

            public MapMethodInfo(
                Type sourceType,
                IClassExpressionConfigurator mapperClass,
                MethodInfo createMappingDataMethod,
                MethodInfo mapMethod)
            {
                SourceType = sourceType;
                _createMappingDataMethod = createMappingDataMethod;
                _mapperClass = mapperClass;
                _mapMethod = mapMethod;

                TargetType = mapMethod
                    .GetParameters()[0]
                    .ParameterType
                    .GetGenericTypeArguments()[1];
            }

            public string RuleSetName => _mapMethod.Name;

            public Type SourceType { get; }

            public Type TargetType { get; }

            public Expression CreateMapCall(Func<Type, Expression> targetFactory)
            {
                var createMappingDataCall = Call(
                    _mapperClass.ThisInstanceExpression,
                    _createMappingDataMethod.MakeGenericMethod(TargetType),
                    targetFactory.Invoke(TargetType),
                    _doNotCreateMapper);

                return Call(_mapMethod, createMappingDataCall);
            }
        }

        #endregion
    }
}
