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
    using DataSources.Finders;
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

        public IList<IBasicConstructionInfo> GetNewObjectCreationInfos(IObjectMappingData mappingData)
            => (IList<IBasicConstructionInfo>)GetNewObjectCreationInfos(mappingData, out _);

        private IList<IConstructionInfo> GetNewObjectCreationInfos(
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
                .Filter(m => IsFactoryMethod(m, mapperData.TargetInstance.Type));

            return CreateConstructionInfo(
                candidateFactoryMethods,
                fm => new ConstructionDataInfo<MethodInfo>(fm, Expression.Call, key, priority: 1));
        }

        private static bool IsFactoryMethod(MethodInfo method, Type targetType)
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
                .Filter(ctor => IsCandidateCtor(ctor, greediestUnconditionalFactoryInfo));

            return CreateConstructionInfo(
                candidateConstructors,
                ctor => new ConstructionDataInfo<ConstructorInfo>(ctor, Expression.New, key, priority: 0));
        }

        private static bool IsCandidateCtor(MethodBase ctor, IBasicConstructionInfo candidateFactoryMethod)
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

        public Expression GetNewObjectCreation(IObjectMappingData mappingData)
        {
            var cachedInfos = GetNewObjectCreationInfos(mappingData, out var constructionKey);

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

        public interface IBasicConstructionInfo
        {
            bool IsConfigured { get; }
            
            bool IsUnconditional { get; }
            
            int ParameterCount { get; }
            
            int Priority { get; }
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

        private sealed class ConstructionDataInfo<TInvokable> : ConstructionInfoBase
            where TInvokable : MethodBase
        {
            private readonly TInvokable _invokable;
            private readonly Func<TInvokable, IList<Expression>, Expression> _constructionFactory;

            public ConstructionDataInfo(
                TInvokable invokable,
                Func<TInvokable, IList<Expression>, Expression> constructionFactory,
                ConstructionKey key,
                int priority)
            {
                _invokable = invokable;
                _constructionFactory = constructionFactory;

                ArgumentDataSources = GetArgumentDataSources(invokable, key);
                CanBeInvoked = ArgumentDataSources.All(ds => ds.HasValue);
                ParameterCount = ArgumentDataSources.Length;
                Priority = priority;

                if (!CanBeInvoked)
                {
                    return;
                }

                IsUnconditional = !ArgumentDataSources.Any(ds => ds.MapperData.TargetMember.IsComplex && ds.IsConditional);
            }

            private static DataSourceSet[] GetArgumentDataSources(TInvokable invokable, ConstructionKey key)
            {
                return invokable
                    .GetParameters()
                    .ProjectToArray(p =>
                    {
                        var parameterMapperData = new ChildMemberMapperData(
                            key.MappingData.MapperData.TargetMember.Append(Member.ConstructorParameter(p)),
                            key.MappingData.MapperData);

                        var memberMappingData = key.MappingData.GetChildMappingData(parameterMapperData);
                        var dataSources = DataSourceFinder.FindFor(memberMappingData);

                        return dataSources;
                    });
            }

            public DataSourceSet[] ArgumentDataSources { get; }

            public bool CanBeInvoked { get; }

            public void AddTo(IList<IConstructionInfo> constructionInfos, ConstructionKey key)
            {
                if (ParameterCount > 0)
                {
                    var dataSources = key.MappingData.MapperData.DataSourcesByTargetMember;

                    foreach (var dataSourceSet in ArgumentDataSources.Filter(ds => !dataSources.ContainsKey(ds.MapperData.TargetMember)))
                    {
                        dataSources.Add(dataSourceSet.MapperData.TargetMember, dataSourceSet);
                    }
                }

                constructionInfos.AddSorted(this);
            }

            public Expression GetConstructionExpression(IList<Expression> argumentValues)
                => _constructionFactory.Invoke(_invokable, argumentValues);

            public override Construction ToConstruction() => new ConstructionData<TInvokable>(this).Construction;
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

                    argumentValues.Add(dataSources.ValueExpression);

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

            public Construction Construction { get; }
        }

        private class Construction : IConditionallyChainable
        {
            private readonly Expression _construction;
            private ParameterExpression _mappingDataObject;

            public Construction(ConfiguredObjectFactory configuredFactory, IMemberMapperData mapperData)
                : this(configuredFactory.Create(mapperData), configuredFactory.GetConditionOrNull(mapperData))
            {
                UsesMappingDataObjectParameter = configuredFactory.UsesMappingDataObjectParameter;
            }

            private Construction(IList<Construction> constructions)
                : this(constructions.ReverseChain())
            {
                UsesMappingDataObjectParameter = constructions.Any(c => c.UsesMappingDataObjectParameter);
            }

            public Construction(Expression construction, Expression condition = null)
            {
                _construction = construction;
                Condition = condition;
            }

            #region Factory Methods

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

            public Expression Condition { get; }

            Expression IConditionallyChainable.Value => _construction;

            public bool UsesMappingDataObjectParameter { get; }

            public Expression GetConstruction(IObjectMappingData mappingData)
                => _construction.Replace(_mappingDataObject, mappingData.MapperData.MappingDataObject);
        }

        #endregion
    }
}