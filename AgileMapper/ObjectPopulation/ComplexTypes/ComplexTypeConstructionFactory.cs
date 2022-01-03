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
    using Caching.Dictionaries;
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
        private readonly ICache<ConstructionKey, IConstruction> _constructionsCache;

        public ComplexTypeConstructionFactory(CacheSet mapperScopedCacheSet)
        {
            _constructionInfosCache = mapperScopedCacheSet.CreateScoped<ConstructionKey, IList<IConstructionInfo>>();
            _constructionsCache = mapperScopedCacheSet.CreateScoped<ConstructionKey, IConstruction>();
        }

        public IList<IBasicConstructionInfo> GetTargetObjectCreationInfos(IObjectMappingData mappingData)
            => GetTargetObjectCreationInfos(mappingData, out _).ProjectToArray(c => (IBasicConstructionInfo)c);

        private IList<IConstructionInfo> GetTargetObjectCreationInfos(
            IObjectMappingData mappingData,
            out ConstructionKey constructionKey)
        {
            constructionKey = new ConstructionKey(mappingData);

            return _constructionInfosCache.GetOrAdd(constructionKey, key =>
            {
                IList<IConstructionInfo> constructionInfos = new List<IConstructionInfo>();

                AddConfiguredConstructionInfos(
                    constructionInfos,
                    key,
                    out var otherConstructionRequired);

                if (otherConstructionRequired && !key.MapperData.TargetType.IsAbstract())
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
            var mapperData = key.MapperData;

            var configuredFactories = mapperData
                .MapperContext
                .UserConfigurations
                .QueryObjectFactories(mapperData);

            foreach (var configuredFactory in configuredFactories)
            {
                var configuredConstructionInfo = new ConfiguredFactoryInfo(configuredFactory);

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
            var mapperData = key.MapperData;

            var greediestAvailableFactoryInfos = GetGreediestAvailableFactoryInfos(key);
            var greediestUnconditionalFactoryInfo = greediestAvailableFactoryInfos.LastOrDefault(f => f.IsUnconditional);

            var constructors = mapperData.TargetInstance.Type
                .GetPublicInstanceConstructors()
                .ToArray();

            int i;

            for (i = 0; i < greediestAvailableFactoryInfos.Length; ++i)
            {
                greediestAvailableFactoryInfos[i].AddTo(constructionInfos, key);
            }

            if (constructors.Any())
            {
                var greediestAvailableNewingInfos = GetGreediestAvailableNewingInfos(
                    constructors,
                    key,
                    greediestUnconditionalFactoryInfo);

                for (i = 0; i < greediestAvailableNewingInfos.Length; ++i)
                {
                    greediestAvailableNewingInfos[i].AddTo(constructionInfos, key);
                }
            }

            if (constructionInfos.None() && mapperData.TargetMemberIsUserStruct())
            {
                constructionInfos.Add(new StructInfo(mapperData.TargetInstance.Type));
            }
        }

        private static ConstructionDataInfo<MethodInfo>[] GetGreediestAvailableFactoryInfos(ConstructionKey key)
        {
            var mapperData = key.MapperData;

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

            mappingData.MapperData.Context.NeedsMappingData = cachedConstruction.NeedsMappingData;

            var constructionExpression = cachedConstruction.GetConstruction(mappingData.MapperData);

            return constructionExpression;
        }

        public Expression GetFactoryMethodObjectCreationOrNull(IObjectMappingData mappingData)
        {
            var key = new ConstructionKey(mappingData);
            var factoryData = GetGreediestAvailableFactoryInfos(key);

            if (factoryData.None())
            {
                return null;
            }

            return factoryData
                .First()
                .ToConstruction()
                .With(key)
                .GetConstruction(mappingData.MapperData);
        }

        public void Reset() => _constructionsCache.Empty();

        #region Helper Classes

        private class ConstructionKey : SourceMemberTypeDependentKeyBase, IMapperKeyDataOwner
        {
            private readonly MappingRuleSet _ruleSet;
            private readonly IQualifiedMember _sourceMember;
            private readonly QualifiedMember _targetMember;

            public ConstructionKey(IObjectMappingData mappingData)
            {
                MappingData = mappingData;
                _ruleSet = mappingData.RuleSet;
                _sourceMember = mappingData.MapperData.SourceMember;
                _targetMember = mappingData.MapperData.TargetMember;
            }

            public IList<IConstructionInfo> Infos { get; set; }

            public override bool Equals(object obj)
            {
                var otherKey = (ConstructionKey)obj;

                return (otherKey!._ruleSet == _ruleSet) &&
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
            IConstruction ToConstruction();
        }

        private abstract class ConstructionInfoBase : IConstructionInfo
        {
            public bool IsConfigured { get; protected set; }

            public bool IsUnconditional { get; protected set; }

            public int ParameterCount { get; protected set; }

            public int Priority { get; protected set; }

            public virtual bool HasCtorParameterFor(Member targetMember) => false;

            public abstract IConstruction ToConstruction();

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

            public ConfiguredFactoryInfo(ConfiguredObjectFactory configuredFactory)
            {
                _configuredFactory = configuredFactory;
                IsConfigured = true;
                IsUnconditional = !_configuredFactory.HasConfiguredCondition;
            }

            public override IConstruction ToConstruction()
                => new ConfiguredFactoryConstruction(_configuredFactory);
        }

        private abstract class ConstructionDataInfo<TInvokable> : ConstructionInfoBase
            where TInvokable : MethodBase
        {
            private readonly QualifiedMember[] _argumentTargetMembers;

            protected ConstructionDataInfo(
                TInvokable invokable,
                ConstructionKey key,
                int priority)
            {
                var parameters = invokable.GetParameters();

                Priority = priority;
                ParameterCount = parameters.Length;
                _argumentTargetMembers = new QualifiedMember[ParameterCount];
                ArgumentDataSources = new IDataSourceSet[ParameterCount];

                CanBeInvoked = IsUnconditional = true;
                var mappingData = key.MappingData;
                var mapperData = mappingData.MapperData;

                for (var i = 0; i < ParameterCount; ++i)
                {
                    var argumentMember = Member.ConstructorParameter(parameters[i]);

                    var targetMember = _argumentTargetMembers[i] =
                        mapperData.TargetMember.Append(argumentMember);

                    var argumentMapperData = new ChildMemberMapperData(targetMember, mapperData);
                    var argumentMappingData = mappingData.GetChildMappingData(argumentMapperData);

                    var dataSources = ArgumentDataSources[i] = MemberDataSourceSetFactory
                        .CreateFor(new DataSourceFindContext(argumentMappingData));

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
                if (ParameterCount > 0)
                {
                    var mapperData = key.MapperData;

                    for (var i = 0; i < ParameterCount; ++i)
                    {
                        mapperData.MergeTargetMemberDataSources(
                           _argumentTargetMembers[i],
                            ArgumentDataSources[i]);
                    }
                }

                constructionInfos.AddThenSort(this);
            }

            public abstract Expression GetConstructionExpression(IList<Expression> argumentValues);

            public override IConstruction ToConstruction() => new ConstructionData<TInvokable>(this).Construction;
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

            public override IConstruction ToConstruction()
                => new Construction(Expression.New(_targetType), replaceValues: false);
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
                    Construction = new Construction(constructionExpression, replaceValues: false);
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

                for (var i = 0; i < dataSources.Count; ++i)
                {
                    var dataSource = dataSources[i];

                    if (!dataSource.IsConditional)
                    {
                        continue;
                    }

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

        private interface IConstruction
        {
            bool NeedsMappingData { get; }

            Expression GetConditionOrNull(IMemberMapperData mapperData);

            Expression GetConstruction(IMemberMapperData mapperData);

            IConstruction With(ConstructionKey key);
        }

        private class ConfiguredFactoryConstruction : IConstruction
        {
            private readonly ConfiguredObjectFactory _configuredFactory;

            public ConfiguredFactoryConstruction(
                ConfiguredObjectFactory configuredFactory)
            {
                _configuredFactory = configuredFactory;
            }

            public bool NeedsMappingData => _configuredFactory.NeedsMappingData;

            public Expression GetConditionOrNull(IMemberMapperData mapperData)
                => _configuredFactory.GetConditionOrNull(mapperData);

            public Expression GetConstruction(IMemberMapperData mapperData)
                => _configuredFactory.Create(mapperData);

            public IConstruction With(ConstructionKey key) => this;
        }

        private class Construction : IConstruction
        {
            private readonly Expression _construction;
            private readonly Expression _condition;
            private readonly bool _replaceValues;
            private IMemberMapperData _mapperData;

            public Construction(
                Expression construction,
                Expression condition = null,
                bool replaceValues = true,
                bool needsMappingData = false)
            {
                _construction = construction;
                _condition = condition;
                _replaceValues = replaceValues;
                NeedsMappingData = needsMappingData;
            }

            #region Factory Methods

            public static IConstruction For(IList<IConstruction> constructions, ConstructionKey key)
            {
                if (constructions.HasOne())
                {
                    return constructions.First().With(key);
                }

                var construction = new Construction(
                    ReverseChain(constructions, key.MapperData),
                    needsMappingData: constructions.Any(c => c.NeedsMappingData));

                return construction.With(key);
            }

            private static Expression ReverseChain(
                IList<IConstruction> constructions,
                IMemberMapperData mapperData)
            {
                return constructions.Chain(
                    cs => cs.Last(),
                    item => item.GetConstruction(mapperData),
                   (valueSoFar, item) => Expression.Condition(
                       item.GetConditionOrNull(mapperData),
                       item.GetConstruction(mapperData),
                       valueSoFar),
                    i => i.Reverse());
            }

            public IConstruction With(ConstructionKey key)
            {
                _mapperData = key.MapperData;
                return this;
            }

            #endregion

            public bool NeedsMappingData { get; }

            public Expression GetConditionOrNull(IMemberMapperData mapperData)
                => _condition != null ? ReplaceValuesIn(_condition, mapperData) : null;

            public Expression GetConstruction(IMemberMapperData mapperData)
            {
                return _replaceValues
                    ? ReplaceValuesIn(_construction, mapperData)
                    : _construction;
            }

            private Expression ReplaceValuesIn(
                Expression expression,
                IMemberMapperData mapperData)
            {
                if (_mapperData == null || mapperData == _mapperData)
                {
                    return expression;
                }

                var replacements = FixedSizeExpressionReplacementDictionary
                    .WithEqualKeys(5)
                    .Add(_mapperData.SourceObject, mapperData.SourceObject)
                    .Add(_mapperData.TargetObject, mapperData.TargetObject)
                    .Add(_mapperData.CreatedObject, mapperData.CreatedObject)
                    .Add(_mapperData.ElementIndex, mapperData.ElementIndex)
                    .Add(_mapperData.ElementKey, mapperData.ElementKey);

                return expression.Replace(replacements);
            }
        }

        #endregion
    }
}