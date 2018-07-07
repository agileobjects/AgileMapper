namespace AgileObjects.AgileMapper.ObjectPopulation.ComplexTypes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Caching;
    using DataSources;
    using DataSources.Finders;
    using Extensions.Internal;
    using MapperKeys;
    using Members;
    using NetStandardPolyfills;
#if NET35
    using Extensions;
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

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
                    out var newingConstructorRequired);

                if (newingConstructorRequired && !key.MappingData.MapperData.TargetType.IsAbstract())
                {
                    AddNewingConstruction(constructions, key);
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

        private static void AddConfiguredConstructions(
            ICollection<Construction> constructions,
            ConstructionKey key,
            out bool newingConstructorRequired)
        {
            var mapperData = key.MappingData.MapperData;

            newingConstructorRequired = true;

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
                    newingConstructorRequired = false;
                    return;
                }
            }
        }

        private static void AddNewingConstruction(ICollection<Construction> constructions, ConstructionKey key)
        {
            var mapperData = key.MappingData.MapperData;

            var constructors = mapperData.TargetInstance.Type
                .GetPublicInstanceConstructors()
                .ToArray();

            var greediestAvailableConstructor = constructors.Any()
                ? constructors
                    .Filter(IsNotCopyConstructor)
                    .Project(ctor => CreateConstructorData(ctor, key))
                    .Filter(ctor => ctor.CanBeConstructed)
                    .OrderByDescending(ctor => ctor.NumberOfParameters)
                    .FirstOrDefault()
                : null;

            if (greediestAvailableConstructor == null)
            {
                if (constructors.None() && mapperData.TargetMemberIsUserStruct())
                {
                    constructions.Add(Construction.NewStruct(mapperData.TargetInstance.Type));
                }

                return;
            }

            foreach (var memberAndDataSourceSet in greediestAvailableConstructor.ArgumentDataSources)
            {
                key.MappingData.MapperData.DataSourcesByTargetMember.Add(
                    memberAndDataSourceSet.Item1,
                    memberAndDataSourceSet.Item2);
            }

            constructions.Add(greediestAvailableConstructor.Construction);
        }

        private static bool IsNotCopyConstructor(ConstructorInfo ctor)
        {
            // If the constructor takes an instance of itself, we'll potentially end 
            // up in an infinite loop figuring out how to create instances for it:
            return ctor.GetParameters().None(p => p.ParameterType == ctor.DeclaringType);
        }

        private static ConstructorData CreateConstructorData(ConstructorInfo ctor, ConstructionKey key)
        {
            var mapperData = key.MappingData.MapperData;

            var ctorData = new ConstructorData(
                ctor,
                ctor.GetParameters()
                    .Project(p =>
                    {
                        var parameterMapperData = new ChildMemberMapperData(
                            mapperData.TargetMember.Append(Member.ConstructorParameter(p)),
                            mapperData);

                        var memberMappingData = key.MappingData.GetChildMappingData(parameterMapperData);
                        var dataSources = DataSourceFinder.FindFor(memberMappingData);

                        return Tuple.Create(memberMappingData.MapperData.TargetMember, dataSources);
                    })
                    .ToArray());

            return ctorData;
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

        private class ConstructorData
        {
            public ConstructorData(
                ConstructorInfo constructor,
                ICollection<Tuple<QualifiedMember, DataSourceSet>> argumentDataSources)
            {
                CanBeConstructed = argumentDataSources.All(ds => ds.Item2.HasValue);
                NumberOfParameters = argumentDataSources.Count;

                if (!CanBeConstructed)
                {
                    return;
                }

                var variables = new List<ParameterExpression>();
                var argumentValues = new List<Expression>(NumberOfParameters);

                foreach (var argumentDataSource in argumentDataSources)
                {
                    variables.AddRange(argumentDataSource.Item2.Variables);
                    argumentValues.Add(argumentDataSource.Item2.ValueExpression);
                }

                var objectConstruction = Expression.New(constructor, argumentValues);

                ArgumentDataSources = argumentDataSources;

                Construction = variables.None()
                    ? new Construction(objectConstruction)
                    : new Construction(Expression.Block(variables, objectConstruction));
            }

            public bool CanBeConstructed { get; }

            public int NumberOfParameters { get; }

            public IEnumerable<Tuple<QualifiedMember, DataSourceSet>> ArgumentDataSources { get; }

            public Construction Construction { get; }
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

            private Construction With(ConstructionKey key)
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