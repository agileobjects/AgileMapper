namespace AgileObjects.AgileMapper.ObjectPopulation.ComplexTypes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Caching;
    using Configuration;
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
                    AddAutoConstructions(constructions, key);
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
            var factoryData = GetGreediestAvailableFactories(key);

            return factoryData.Any()
                ? factoryData.First().Construction.With(key).GetConstruction(mappingData)
                : null;
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

        private static void AddAutoConstructions(IList<Construction> constructions, ConstructionKey key)
        {
            var mapperData = key.MappingData.MapperData;

            var greediestAvailableFactories = GetGreediestAvailableFactories(key);
            var greediestUnconditionalFactory = greediestAvailableFactories.LastOrDefault(f => f.IsUnconditional);

            var constructors = mapperData.TargetInstance.Type
                .GetPublicInstanceConstructors()
                .ToArray();

            var greediestAvailableNewings = constructors.Any()
                ? GetGreediestAvailableNewings(constructors, key, greediestUnconditionalFactory)
                : Enumerable<ConstructionData<ConstructorInfo>>.EmptyArray;

            int i;

            for (i = 0; i < greediestAvailableFactories.Length; i++)
            {
                greediestAvailableFactories[i].AddTo(constructions, key);
            }

            for (i = 0; i < greediestAvailableNewings.Length; i++)
            {
                greediestAvailableNewings[i].AddTo(constructions, key);
            }

            if (constructions.None() && mapperData.TargetMemberIsUserStruct())
            {
                constructions.Add(Construction.NewStruct(mapperData.TargetInstance.Type));
            }
        }

        private static ConstructionData<MethodInfo>[] GetGreediestAvailableFactories(ConstructionKey key)
        {
            var mapperData = key.MappingData.MapperData;

            var candidateFactoryMethods = mapperData.TargetInstance.Type
                .GetPublicStaticMethods()
                .Filter(m => IsFactoryMethod(m, mapperData.TargetInstance.Type));

            return CreateConstructionData(
                candidateFactoryMethods,
                fm => new ConstructionData<MethodInfo>(fm, Expression.Call, key, priority: 1));
        }

        private static bool IsFactoryMethod(MethodInfo method, Type targetType)
        {
            return (method.ReturnType == targetType) &&
                   (method.Name.StartsWith("Create", Ordinal) || method.Name.StartsWith("Get", Ordinal));
        }

        private static ConstructionData<ConstructorInfo>[] GetGreediestAvailableNewings(
            IEnumerable<ConstructorInfo> constructors,
            ConstructionKey key,
            ConstructionData<MethodInfo> greediestUnconditionalFactory)
        {
            var candidateConstructors = constructors
                .Filter(ctor => IsCandidateCtor(ctor, greediestUnconditionalFactory));

            return CreateConstructionData(
                candidateConstructors,
                ctor => new ConstructionData<ConstructorInfo>(ctor, Expression.New, key, priority: 0));
        }

        private static bool IsCandidateCtor(MethodBase ctor, ConstructionData<MethodInfo> candidateFactoryMethod)
        {
            var ctorCarameters = ctor.GetParameters();

            return ((candidateFactoryMethod == null) ||
                    (candidateFactoryMethod.ParameterCount < ctorCarameters.Length)) &&
                     IsNotCopyConstructor(ctor.DeclaringType, ctorCarameters);
        }

        private static bool IsNotCopyConstructor(Type type, IList<ParameterInfo> ctorParameters)
        {
            // If the constructor takes an instance of itself, we'll potentially end 
            // up in an infinite loop figuring out how to create instances for it:
            return ctorParameters.None(p => p.ParameterType == type);
        }

        private static ConstructionData<T>[] CreateConstructionData<T>(
            IEnumerable<T> invokables,
            Func<T, ConstructionData<T>> dataFactory)
            where T : MethodBase
        {
            return invokables
                .OrderByDescending(fm => fm.GetParameters().Length)
                .Project(dataFactory.Invoke)
                .Filter(fm => fm.CanBeInvoked)
                .TakeUntil(fm => fm.IsUnconditional)
                .ToArray();
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

        private class ConstructionData<TInvokable> : IConstructionInfo
            where TInvokable : MethodBase
        {
            private readonly Tuple<QualifiedMember, DataSourceSet>[] _argumentDataSources;

            public ConstructionData(
                TInvokable invokable,
                Func<TInvokable, IList<Expression>, Expression> constructionFactory,
                ConstructionKey key,
                int priority)
            {
                var argumentDataSources = GetArgumentDataSources(invokable, key);

                CanBeInvoked = argumentDataSources.All(ds => ds.Item2.HasValue);
                ParameterCount = argumentDataSources.Length;
                Priority = priority;

                if (!CanBeInvoked)
                {
                    return;
                }

                IsUnconditional = true;

                Expression constructionExpression;

                if (argumentDataSources.None())
                {
                    constructionExpression = constructionFactory.Invoke(invokable, Enumerable<Expression>.EmptyArray);
                    Construction = new Construction(this, constructionExpression);
                    return;
                }

                _argumentDataSources = argumentDataSources;

                var variables = new List<ParameterExpression>();
                var argumentValues = new List<Expression>(ParameterCount);
                var condition = default(Expression);

                foreach (var argumentDataSource in argumentDataSources)
                {
                    var dataSources = argumentDataSource.Item2;

                    variables.AddRange(dataSources.Variables);
                    argumentValues.Add(dataSources.ValueExpression);

                    if (!argumentDataSource.Item1.IsComplex || !dataSources.IsConditional)
                    {
                        continue;
                    }

                    IsUnconditional = false;

                    var dataSourceCondition = BuildConditions(dataSources);

                    if (condition == null)
                    {
                        condition = dataSourceCondition;
                        continue;
                    }

                    condition = Expression.AndAlso(condition, dataSourceCondition);
                }

                constructionExpression = constructionFactory.Invoke(invokable, argumentValues);

                Construction = variables.None()
                    ? new Construction(this, constructionExpression, condition)
                    : new Construction(this, Expression.Block(variables, constructionExpression), condition);
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

            private static Expression BuildConditions(DataSourceSet dataSources)
            {
                var conditions = default(Expression);

                foreach (var dataSource in dataSources.Filter(ds => ds.IsConditional))
                {
                    if (conditions == null)
                    {
                        conditions = dataSource.Condition;
                        continue;
                    }

                    conditions = Expression.OrElse(conditions, dataSource.Condition);
                }

                return conditions;
            }

            public bool CanBeInvoked { get; }

            public bool IsUnconditional { get; }

            public int ParameterCount { get; }

            public int Priority { get; }

            public Construction Construction { get; }

            public void AddTo(IList<Construction> constructions, ConstructionKey key)
            {
                if (ParameterCount > 0)
                {
                    var dataSources = key.MappingData.MapperData.DataSourcesByTargetMember;

                    foreach (var memberAndDataSourceSet in _argumentDataSources.Filter(ads => !dataSources.ContainsKey(ads.Item1)))
                    {
                        dataSources.Add(memberAndDataSourceSet.Item1, memberAndDataSourceSet.Item2);
                    }
                }

                constructions.AddSorted(Construction);
            }
        }

        private interface IConstructionInfo
        {
            int ParameterCount { get; }

            int Priority { get; }
        }

        private class Construction : IConditionallyChainable, IComparable<Construction>
        {
            private readonly Expression _construction;
            private readonly bool _isConfigured;
            private readonly IConstructionInfo _info;
            private ParameterExpression _mappingDataObject;

            public Construction(ConfiguredObjectFactory configuredFactory, IMemberMapperData mapperData)
                : this(configuredFactory.Create(mapperData), configuredFactory.GetConditionOrNull(mapperData))
            {
                UsesMappingDataObjectParameter = configuredFactory.UsesMappingDataObjectParameter;
                _isConfigured = true;
            }

            public Construction(IConstructionInfo info, Expression construction, Expression condition = null)
                : this(construction, condition)
            {
                _info = info;
            }

            private Construction(IList<Construction> constructions)
                : this(constructions.ReverseChain())
            {
                UsesMappingDataObjectParameter = constructions.Any(c => c.UsesMappingDataObjectParameter);
            }

            private Construction(Expression construction, Expression condition = null)
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

            public int CompareTo(Construction other)
            {
                // ReSharper disable once ImpureMethodCallOnReadonlyValueField
                var isConfiguredComparison = other._isConfigured.CompareTo(_isConfigured);

                if (isConfiguredComparison != 0)
                {
                    return isConfiguredComparison;
                }

                var conditionalComparison = IsUnconditional.CompareTo(other.IsUnconditional);

                if (conditionalComparison != 0)
                {
                    return conditionalComparison;
                }

                var paramCountComparison = _info.ParameterCount.CompareTo(other._info.ParameterCount);

                if (paramCountComparison != 0)
                {
                    return paramCountComparison;
                }

                var priorityComparison = other._info.Priority.CompareTo(_info.Priority);

                return priorityComparison;
            }
        }

        #endregion
    }
}