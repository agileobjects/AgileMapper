namespace AgileObjects.AgileMapper.ObjectPopulation.ComplexTypes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Caching;
    using DataSources;
    using DataSources.Finders;
    using Extensions;
    using Extensions.Internal;
    using MapperKeys;
    using Members;
    using NetStandardPolyfills;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using static System.StringComparison;

    internal class ComplexTypeConstructionFactory
    {
        private readonly ICache<ConstructionKey, Construction> _constructorsCache;

        public ComplexTypeConstructionFactory(CacheSet mapperScopedCacheSet)
        {
            _constructorsCache = mapperScopedCacheSet.CreateScoped<ConstructionKey, Construction>();
        }

        public Expression GetNewObjectCreation(IObjectMappingData mappingData)
        {
            var objectCreation = _constructorsCache.GetOrAdd(new ConstructionKey(mappingData), key =>
            {
                var constructions = new List<Construction>();

                AddConfiguredConstructions(
                    constructions,
                    key,
                    out var otherConstructionRequired);

                if (otherConstructionRequired && !key.MappingData.MapperData.TargetType.IsAbstract())
                {
                    AddAutoConstruction(constructions, key);
                }

                if (constructions.None())
                {
                    key.MappingData = null;
                    return null;
                }

                var construction = Construction.For(constructions, key);

                key.AddSourceMemberTypeTesterIfRequired();
                key.MappingData = null;

                return construction;
            });

            if (objectCreation == null)
            {
                return null;
            }

            mappingData.MapperData.Context.UsesMappingDataObjectAsParameter = objectCreation.UsesMappingDataObjectParameter;

            var creationExpression = objectCreation.GetConstruction(mappingData);

            return creationExpression;
        }

        public Expression GetFactoryMethodObjectCreationOrNull(IObjectMappingData mappingData)
        {
            var key = new ConstructionKey(mappingData);
            var factoryData = GetGreediestAvailableFactoryData(key);

            return factoryData?.Construction.With(key).GetConstruction(mappingData);
        }

        private static void AddConfiguredConstructions(
            ICollection<Construction> constructions,
            ConstructionKey key,
            out bool otherConstructionRequired)
        {
            var mapperData = key.MappingData.MapperData;

            otherConstructionRequired = true;

            var configuredFactories = mapperData
                .MapperContext
                .UserConfigurations
                .GetObjectFactories(mapperData);

            foreach (var configuredFactory in configuredFactories)
            {
                var configuredConstruction = new Construction(configuredFactory, mapperData);

                constructions.Add(configuredConstruction);

                if (configuredConstruction.IsUnconditional)
                {
                    otherConstructionRequired = false;
                    return;
                }
            }
        }

        private static void AddAutoConstruction(ICollection<Construction> constructions, ConstructionKey key)
        {
            var mapperData = key.MappingData.MapperData;

            var greediestAvailableFactory = GetGreediestAvailableFactoryData(key);

            var constructors = mapperData.TargetInstance.Type
                .GetPublicInstanceConstructors()
                .ToArray();

            var greedierAvailableNewing = constructors.Any()
                ? constructors
                    .Filter(ctor => IsCandidateCtor(ctor, greediestAvailableFactory))
                    .Project(ctor => new ConstructionData<ConstructorInfo>(ctor, Expression.New, key))
                    .Filter(ctor => ctor.CanBeInvoked)
                    .OrderByDescending(ctor => ctor.NumberOfParameters)
                    .FirstOrDefault()
                : null;

            if (greedierAvailableNewing != null)
            {
                greedierAvailableNewing.AddTo(constructions, key);
                return;
            }

            if (greediestAvailableFactory != null)
            {
                greediestAvailableFactory.AddTo(constructions, key);
                return;
            }

            if (constructors.None() && mapperData.TargetMemberIsUserStruct())
            {
                constructions.Add(Construction.NewStruct(mapperData.TargetInstance.Type));
            }
        }

        private static ConstructionData<MethodInfo> GetGreediestAvailableFactoryData(ConstructionKey key)
        {
            var mapperData = key.MappingData.MapperData;

            return mapperData.TargetInstance.Type
                .GetPublicStaticMethods()
                .Filter(m => IsFactoryMethod(m, mapperData.TargetInstance.Type))
                .Project(fm => new ConstructionData<MethodInfo>(fm, Expression.Call, key))
                .Filter(fm => fm.CanBeInvoked)
                .OrderByDescending(fm => fm.NumberOfParameters)
                .FirstOrDefault();
        }

        private static bool IsFactoryMethod(MethodInfo method, Type targetType)
        {
            return (method.ReturnType == targetType) &&
                   (method.Name.StartsWith("Create", Ordinal) || method.Name.StartsWith("Get", Ordinal));
        }

        private static bool IsCandidateCtor(MethodBase ctor, ConstructionData<MethodInfo> candidateFactoryMethod)
        {
            var ctorCarameters = ctor.GetParameters();

            return ((candidateFactoryMethod == null) ||
                    (candidateFactoryMethod.NumberOfParameters < ctorCarameters.Length)) &&
                     IsNotCopyConstructor(ctor.DeclaringType, ctorCarameters);
        }

        private static bool IsNotCopyConstructor(Type type, IList<ParameterInfo> ctorParameters)
        {
            // If the constructor takes an instance of itself, we'll potentially end 
            // up in an infinite loop figuring out how to create instances for it:
            return ctorParameters.None(p => p.ParameterType == type);
        }

        public void Reset() => _constructorsCache.Empty();

        #region Helper Classes

        private class ConstructionKey : SourceMemberTypeDependentKeyBase, IMappingDataOwner
        {
            private readonly MappingRuleSet _ruleSet;
            private readonly IQualifiedMember _sourceMember;
            private readonly QualifiedMember _targetMember;

            public ConstructionKey(IObjectMappingData mappingData)
            {
                MappingData = mappingData;
                _ruleSet = mappingData.MappingContext.RuleSet;
                _sourceMember = mappingData.MapperData.SourceMember;
                _targetMember = mappingData.MapperData.TargetMember;
            }

            public override bool Equals(object obj)
            {
                var otherKey = (ConstructionKey)obj;

                // ReSharper disable once PossibleNullReferenceException
                return (otherKey._ruleSet == _ruleSet) &&
                    (otherKey._sourceMember == _sourceMember) &&
                    (otherKey._targetMember == _targetMember) &&
                     SourceHasRequiredTypes(otherKey);
            }

            #region ExcludeFromCodeCoverage
#if DEBUG
            [ExcludeFromCodeCoverage]
#endif
            #endregion
            public override int GetHashCode() => 0;
        }

        private class ConstructionData<TInvokable>
            where TInvokable : MethodBase
        {
            private readonly IEnumerable<Tuple<QualifiedMember, DataSourceSet>> _argumentDataSources;

            public ConstructionData(
                TInvokable invokable,
                Func<TInvokable, IList<Expression>, Expression> constructionFactory,
                ConstructionKey key)
            {
                var argumentDataSources = GetArgumentDataSources(invokable, key);

                CanBeInvoked = argumentDataSources.All(ds => ds.Item2.HasValue);
                NumberOfParameters = argumentDataSources.Length;

                if (!CanBeInvoked)
                {
                    return;
                }

                IList<ParameterExpression> variables;
                IList<Expression> argumentValues;

                if (argumentDataSources.None())
                {
                    variables = Enumerable<ParameterExpression>.EmptyArray;
                    argumentValues = Enumerable<Expression>.EmptyArray;
                    _argumentDataSources = Enumerable<Tuple<QualifiedMember, DataSourceSet>>.Empty;
                }
                else
                {
                    var vars = new List<ParameterExpression>();
                    argumentValues = new List<Expression>(NumberOfParameters);

                    foreach (var argumentDataSource in argumentDataSources)
                    {
                        vars.AddRange(argumentDataSource.Item2.Variables);
                        argumentValues.Add(argumentDataSource.Item2.ValueExpression);
                    }

                    variables = vars.Any() ? (IList<ParameterExpression>)vars : Enumerable<ParameterExpression>.EmptyArray;
                    _argumentDataSources = argumentDataSources;
                }

                var constructionExpression = constructionFactory.Invoke(invokable, argumentValues);

                Construction = variables.None()
                    ? new Construction(constructionExpression)
                    : new Construction(Expression.Block(variables, constructionExpression));
            }

            private static Tuple<QualifiedMember, DataSourceSet>[] GetArgumentDataSources(TInvokable invokable, ConstructionKey key)
            {
                return invokable
                    .GetParameters()
                    .Project(p =>
                    {
                        var parameterMapperData = new ChildMemberMapperData(
                            key.MappingData.MapperData.TargetMember.Append(Member.ConstructorParameter(p)),
                            key.MappingData.MapperData);

                        var memberMappingData = key.MappingData.GetChildMappingData(parameterMapperData);
                        var dataSources = DataSourceFinder.FindFor(memberMappingData);

                        return Tuple.Create(memberMappingData.MapperData.TargetMember, dataSources);
                    })
                    .ToArray();
            }

            public bool CanBeInvoked { get; }

            public int NumberOfParameters { get; }

            public Construction Construction { get; }

            public void AddTo(ICollection<Construction> constructions, ConstructionKey key)
            {
                if (NumberOfParameters > 0)
                {
                    foreach (var memberAndDataSourceSet in _argumentDataSources)
                    {
                        key.MappingData.MapperData.DataSourcesByTargetMember.Add(
                            memberAndDataSourceSet.Item1,
                            memberAndDataSourceSet.Item2);
                    }
                }

                constructions.Add(Construction);
            }
        }

        private class Construction : IConditionallyChainable
        {
            private readonly Expression _construction;
            private ParameterExpression _mappingDataObject;

            private Construction(IList<Construction> constructions)
                : this(constructions.ReverseChain())
            {
                UsesMappingDataObjectParameter = constructions.Any(c => c.UsesMappingDataObjectParameter);
            }

            public Construction(ConfiguredObjectFactory configuredFactory, IMemberMapperData mapperData)
                : this(configuredFactory.Create(mapperData), configuredFactory.GetConditionOrNull(mapperData))
            {
                UsesMappingDataObjectParameter = configuredFactory.UsesMappingDataObjectParameter;
            }

            public Construction(Expression construction, Expression condition = null)
            {
                _construction = construction;
                Condition = condition;
            }

            #region Factory Methods

            public static Construction NewStruct(Type type)
            {
                var parameterlessNew = Expression.New(type);

                return new Construction(parameterlessNew);
            }

            public static Construction For(IList<Construction> constructions, ConstructionKey key)
            {
                var construction = constructions.HasOne()
                    ? constructions.First()
                    : new Construction(constructions);

                return construction.With(key);
            }

            public Construction With(ConstructionKey key)
            {
                _mappingDataObject = key.MappingData.MapperData.MappingDataObject;
                return this;
            }

            #endregion

            public Expression PreCondition => null;

            public bool IsUnconditional => Condition == null;

            public Expression Condition { get; }

            Expression IConditionallyChainable.Value => _construction;

            public bool UsesMappingDataObjectParameter { get; }

            public Expression GetConstruction(IObjectMappingData mappingData)
                => _construction.Replace(_mappingDataObject, mappingData.MapperData.MappingDataObject);
        }

        #endregion
    }
}