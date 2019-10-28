namespace AgileObjects.AgileMapper.ObjectPopulation.ComplexTypes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using System.Reflection;
    using Caching;
    using Configuration;
    using DataSources;
    using DataSources.Factories;
    using Extensions;
    using Extensions.Internal;
    using MapperKeys;
    using Members;
    using NetStandardPolyfills;
    using static System.StringComparison;

    internal class ComplexTypeConstructionFactory
    {
        private readonly ICache<ConstructionKey, IList<IConstructionInfo>> _constructionInfosCache;
        private readonly ICache<ConstructionKey, Construction> _constructionsCache;

        public ComplexTypeConstructionFactory(CacheSet mapperScopedCacheSet)
        {
            _constructionInfosCache = mapperScopedCacheSet.CreateScoped<ConstructionKey, IList<IConstructionInfo>>();
            _constructionsCache = mapperScopedCacheSet.CreateScoped<ConstructionKey, Construction>();
        }

        public IList<IBasicConstructionInfo> GetTargetObjectCreationInfos(IObjectMappingData mappingData)
            => GetTargetObjectCreationInfos(mappingData, out _).ProjectToArray(c => (IBasicConstructionInfo)c);

        private IList<IConstructionInfo> GetTargetObjectCreationInfos(
            IObjectMappingData mappingData,
            out ConstructionKey constructionKey)
        {
            return _constructionInfosCache.GetOrAdd(constructionKey = new ConstructionKey(mappingData), key =>
            {
                IList<IConstructionInfo> constructionInfos = new List<IConstructionInfo>();

                AddConfiguredConstructionInfos(
                    constructionInfos,
                    key,
                    out var otherConstructionRequired);

                if (otherConstructionRequired && !key.MappingData.MapperData.TargetType.IsAbstract())
                {
                    AddAutoConstructionInfos(constructionInfos, key);
                }

                key.AddSourceMemberTypeTesterIfRequired();
                key.MappingData = null;

                return constructionInfos.None()
                    ? Enumerable<IConstructionInfo>.EmptyArray
                    : constructionInfos;
            });
        }

        private static void AddConfiguredConstructionInfos(
            ICollection<IConstructionInfo> constructionInfos,
            ConstructionKey key,
            out bool otherConstructionRequired)
        {
            var mapperData = key.MappingData.MapperData;

            var configuredFactories = mapperData
                .MapperContext
                .UserConfigurations
                .GetObjectFactories(mapperData);

            foreach (var configuredFactory in configuredFactories)
            {
                var configuredConstructionInfo = new ConfiguredFactoryInfo(configuredFactory, mapperData);

                constructionInfos.Add(configuredConstructionInfo);

                if (configuredConstructionInfo.IsUnconditional)
                {
                    otherConstructionRequired = false;
                    return;
                }
            }

            otherConstructionRequired = true;
        }

        private static void AddAutoConstructionInfos(IList<IConstructionInfo> constructionInfos, ConstructionKey key)
        {
            var mapperData = key.MappingData.MapperData;

            var greediestAvailableFactoryInfos = GetGreediestAvailableFactoryInfos(key);
            var greediestUnconditionalFactoryInfo = greediestAvailableFactoryInfos.LastOrDefault(f => f.IsUnconditional);

            var constructors = mapperData.TargetInstance.Type
                .GetPublicInstanceConstructors()
                .ToArray();

            int i;

            for (i = 0; i < greediestAvailableFactoryInfos.Length;)
            {
                greediestAvailableFactoryInfos[i++].AddTo(constructionInfos, key);
            }

            if (constructors.Any())
            {
                var greediestAvailableNewingInfos = GetGreediestAvailableNewingInfos(
                    constructors,
                    key,
                    greediestUnconditionalFactoryInfo);

                for (i = 0; i < greediestAvailableNewingInfos.Length;)
                {
                    greediestAvailableNewingInfos[i++].AddTo(constructionInfos, key);
                }
            }

            if (constructionInfos.None() && mapperData.TargetMemberIsUserStruct())
            {
                constructionInfos.Add(new StructInfo(mapperData.TargetInstance.Type));
            }
        }

        private static ConstructionDataInfo<MethodInfo>[] GetGreediestAvailableFactoryInfos(ConstructionKey key)
        {
            var mapperData = key.MappingData.MapperData;

            var candidateFactoryMethods = mapperData.TargetInstance.Type
                .GetPublicStaticMethods()
                .Filter(mapperData.TargetInstance.Type, IsFactoryMethod);

            return CreateConstructionInfo(candidateFactoryMethods, fm => new FactoryMethodInfo(fm, key));
        }

        private static bool IsFactoryMethod(Type targetType, MethodInfo method)
        {
            return (method.ReturnType == targetType) &&
                   (method.Name.StartsWith("Create", Ordinal) || method.Name.StartsWith("Get", Ordinal));
        }

        private static ConstructionDataInfo<ConstructorInfo>[] GetGreediestAvailableNewingInfos(
            IEnumerable<ConstructorInfo> constructors,
            ConstructionKey key,
            IBasicConstructionInfo greediestUnconditionalFactoryInfo)
        {
            var candidateConstructors = constructors
                .Filter(greediestUnconditionalFactoryInfo, IsCandidateCtor);

            return CreateConstructionInfo(candidateConstructors, ctor => new ObjectNewingInfo(ctor, key));
        }

        private static bool IsCandidateCtor(IBasicConstructionInfo candidateFactoryMethod, MethodBase ctor)
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
            return ctorParameters.None(type, (t, p) => p.ParameterType == t);
        }

        private static ConstructionDataInfo<T>[] CreateConstructionInfo<T>(
            IEnumerable<T> invokables,
            Func<T, ConstructionDataInfo<T>> dataFactory)
            where T : MethodBase
        {
            return invokables
                .OrderByDescending(fm => fm.GetParameters().Length)
                .Project(dataFactory.Invoke)
                .Filter(fm => fm.CanBeInvoked)
                .TakeUntil(fm => fm.IsUnconditional)
                .ToArray();
        }

        public Expression GetTargetObjectCreation(IObjectMappingData mappingData)
        {
            var cachedInfos = GetTargetObjectCreationInfos(mappingData, out var constructionKey);

            if (cachedInfos.None())
            {
                return null;
            }

            constructionKey.MappingData = mappingData;
            constructionKey.Infos = cachedInfos;

            var cachedConstruction = _constructionsCache.GetOrAdd(constructionKey, key =>
            {
                var constructions = key.Infos.ProjectToArray(info => info.ToConstruction());
                var construction = Construction.For(constructions, key);

                key.AddSourceMemberTypeTesterIfRequired();
                key.MappingData = null;

                return construction;
            });

            mappingData.MapperData.Context.UsesMappingDataObjectAsParameter = cachedConstruction.UsesMappingDataObjectParameter;

            var constructionExpression = cachedConstruction.GetConstruction(mappingData);

            return constructionExpression;
        }

        public Expression GetFactoryMethodObjectCreationOrNull(IObjectMappingData mappingData)
        {
            var key = new ConstructionKey(mappingData);
            var factoryData = GetGreediestAvailableFactoryInfos(key);

            return factoryData.Any()
                ? factoryData.First().ToConstruction().With(key).GetConstruction(mappingData)
                : null;
        }

        public void Reset() => _constructionsCache.Empty();

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

            public IList<IConstructionInfo> Infos { get; set; }

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

        private interface IConstructionInfo : IBasicConstructionInfo, IComparable<IConstructionInfo>
        {
            Construction ToConstruction();
        }

        private abstract class ConstructionInfoBase : IConstructionInfo
        {
            public bool IsConfigured { get; protected set; }

            public bool IsUnconditional { get; protected set; }

            public int ParameterCount { get; protected set; }

            public int Priority { get; protected set; }

            public virtual bool HasCtorParameterFor(Member targetMember) => false;

            public abstract Construction ToConstruction();

            public int CompareTo(IConstructionInfo other)
            {
                // ReSharper disable once ImpureMethodCallOnReadonlyValueField
                var isConfiguredComparison = other.IsConfigured.CompareTo(IsConfigured);

                if (isConfiguredComparison != 0)
                {
                    return isConfiguredComparison;
                }

                var conditionalComparison = IsUnconditional.CompareTo(other.IsUnconditional);

                if (conditionalComparison != 0)
                {
                    return conditionalComparison;
                }

                var paramCountComparison = ParameterCount.CompareTo(other.ParameterCount);

                if (paramCountComparison != 0)
                {
                    return paramCountComparison;
                }

                var priorityComparison = other.Priority.CompareTo(Priority);

                return priorityComparison;
            }
        }

        private sealed class ConfiguredFactoryInfo : ConstructionInfoBase
        {
            private readonly ConfiguredObjectFactory _configuredFactory;
            private readonly IMemberMapperData _mapperData;

            public ConfiguredFactoryInfo(ConfiguredObjectFactory configuredFactory, IMemberMapperData mapperData)
            {
                _configuredFactory = configuredFactory;
                _mapperData = mapperData;
                IsConfigured = true;
                IsUnconditional = !_configuredFactory.HasConfiguredCondition;
            }

            public override Construction ToConstruction() => new Construction(_configuredFactory, _mapperData);
        }

        private abstract class ConstructionDataInfo<TInvokable> : ConstructionInfoBase
            where TInvokable : MethodBase
        {
            private readonly IMemberMapperData[] _argumentMapperDatas;

            protected ConstructionDataInfo(
                TInvokable invokable,
                ConstructionKey key,
                int priority)
            {
                var parameters = invokable.GetParameters();

                Priority = priority;
                ParameterCount = parameters.Length;
                ArgumentDataSources = new IDataSourceSet[ParameterCount];
                _argumentMapperDatas = new IMemberMapperData[ParameterCount];

                CanBeInvoked = IsUnconditional = true;
                var mappingData = key.MappingData;

                for (var i = 0; i < ParameterCount; ++i)
                {
                    var argumentMember = Member.ConstructorParameter(parameters[i]);

                    var argumentMapperData = _argumentMapperDatas[i] = new ChildMemberMapperData(
                        mappingData.MapperData.TargetMember.Append(argumentMember),
                        mappingData.MapperData);

                    var memberMappingData = mappingData.GetChildMappingData(argumentMapperData);

                    var dataSources = ArgumentDataSources[i] = MemberDataSourceSetFactory
                        .CreateFor(new DataSourceFindContext(memberMappingData));

                    if (CanBeInvoked && !dataSources.HasValue)
                    {
                        CanBeInvoked = false;
                    }

                    if (IsUnconditional && dataSources.IsConditional && argumentMember.IsComplex)
                    {
                        IsUnconditional = false;
                    }
                }
            }

            public IDataSourceSet[] ArgumentDataSources { get; }

            public bool CanBeInvoked { get; }

            public void AddTo(IList<IConstructionInfo> constructionInfos, ConstructionKey key)
            {
                if (ParameterCount == 0)
                {
                    constructionInfos.AddThenSort(this);
                    return;
                }

                var dataSources = key.MappingData.MapperData.DataSourcesByTargetMember;

                for (var i = 0; i < ParameterCount; ++i)
                {
                    var targetMember = _argumentMapperDatas[i].TargetMember;

                    if (!dataSources.ContainsKey(targetMember))
                    {
                        dataSources.Add(targetMember, ArgumentDataSources[i]);
                    }
                }

                constructionInfos.AddThenSort(this);
            }

            public abstract Expression GetConstructionExpression(IList<Expression> argumentValues);

            public override Construction ToConstruction() => new ConstructionData<TInvokable>(this).Construction;
        }

        private sealed class ObjectNewingInfo : ConstructionDataInfo<ConstructorInfo>
        {
            private readonly ConstructorInfo _ctor;
            private readonly string[] _parameterNames;

            public ObjectNewingInfo(ConstructorInfo ctor, ConstructionKey key)
                : base(ctor, key, priority: 0)
            {
                _ctor = ctor;
                _parameterNames = _ctor.GetParameters().ProjectToArray(p => p.Name);
            }

            public override bool HasCtorParameterFor(Member targetMember)
                => _parameterNames.Contains(targetMember.Name, StringComparer.OrdinalIgnoreCase);

            public override Expression GetConstructionExpression(IList<Expression> argumentValues)
                => Expression.New(_ctor, argumentValues);
        }

        private sealed class FactoryMethodInfo : ConstructionDataInfo<MethodInfo>
        {
            private readonly MethodInfo _factoryMethod;

            public FactoryMethodInfo(MethodInfo factoryMethod, ConstructionKey key)
                : base(factoryMethod, key, priority: 1)
            {
                _factoryMethod = factoryMethod;
            }

            public override Expression GetConstructionExpression(IList<Expression> argumentValues)
                => Expression.Call(_factoryMethod, argumentValues);
        }

        private sealed class StructInfo : ConstructionInfoBase
        {
            private readonly Type _targetType;

            public StructInfo(Type targetType)
            {
                _targetType = targetType;
                IsUnconditional = true;
            }

            public override Construction ToConstruction() => new Construction(Expression.New(_targetType));
        }

        private sealed class ConstructionData<TInvokable>
            where TInvokable : MethodBase
        {
            public ConstructionData(ConstructionDataInfo<TInvokable> info)
            {
                Expression constructionExpression;

                if (info.ArgumentDataSources.None())
                {
                    constructionExpression = info.GetConstructionExpression(Enumerable<Expression>.EmptyArray);
                    Construction = new Construction(constructionExpression);
                    return;
                }

                var variables = default(List<ParameterExpression>);
                var argumentValues = new List<Expression>(info.ParameterCount);
                var condition = default(Expression);

                foreach (var dataSources in info.ArgumentDataSources)
                {
                    if (dataSources.Variables.Any())
                    {
                        if (variables == null)
                        {
                            variables = new List<ParameterExpression>(dataSources.Variables);
                        }
                        else
                        {
                            variables.AddRange(dataSources.Variables);
                        }
                    }

                    argumentValues.Add(dataSources.BuildValue());

                    if (info.IsUnconditional)
                    {
                        continue;
                    }

                    var dataSourceCondition = BuildConditions(dataSources);

                    if (condition == null)
                    {
                        condition = dataSourceCondition;
                        continue;
                    }

                    condition = Expression.AndAlso(condition, dataSourceCondition);
                }

                constructionExpression = info.GetConstructionExpression(argumentValues);

                Construction = variables.NoneOrNull()
                    ? new Construction(constructionExpression, condition)
                    : new Construction(Expression.Block(variables, constructionExpression), condition);
            }

            private static Expression BuildConditions(IDataSourceSet dataSources)
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

            public Construction Construction { get; }
        }

        private class Construction
        {
            private readonly Expression _condition;
            private readonly Expression _construction;
            private ParameterExpression _mappingDataObject;

            public Construction(ConfiguredObjectFactory configuredFactory, IMemberMapperData mapperData)
                : this(configuredFactory.Create(mapperData), configuredFactory.GetConditionOrNull(mapperData))
            {
                UsesMappingDataObjectParameter = configuredFactory.UsesMappingDataObjectParameter;
            }

            public Construction(
                Expression construction,
                Expression condition = null,
                bool usesMappingDataObjectParameter = false)
            {
                _construction = construction;
                _condition = condition;
                UsesMappingDataObjectParameter = usesMappingDataObjectParameter;
            }

            #region Factory Methods

            public static Construction For(IList<Construction> constructions, ConstructionKey key)
            {
                if (constructions.HasOne())
                {
                    return constructions.First().With(key);
                }

                var construction = new Construction(
                    ReverseChain(constructions),
                    usesMappingDataObjectParameter: constructions.Any(c => c.UsesMappingDataObjectParameter));

                return construction.With(key);
            }

            private static Expression ReverseChain(IList<Construction> constructions)
            {
                return constructions.Chain(
                    cs => cs.Last(),
                    item => item._construction,
                    (valueSoFar, item) => Expression.Condition(item._condition, item._construction, valueSoFar),
                    i => i.Reverse());
            }

            public Construction With(ConstructionKey key)
            {
                _mappingDataObject = key.MappingData.MapperData.MappingDataObject;
                return this;
            }

            #endregion

            public bool UsesMappingDataObjectParameter { get; }

            public Expression GetConstruction(IObjectMappingData mappingData)
                => _construction.Replace(_mappingDataObject, mappingData.MapperData.MappingDataObject);
        }

        #endregion
    }
}