namespace AgileObjects.AgileMapper.ObjectPopulation.ComplexTypes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Caching;
    using DataSources;
    using Extensions;
    using Members;
    using NetStandardPolyfills;

    internal class ComplexTypeConstructionFactory
    {
        private readonly ICache<ConstructionKey, Construction> _constructorsCache;

        public ComplexTypeConstructionFactory(MapperContext mapperContext)
        {
            _constructorsCache = mapperContext.Cache.CreateScoped<ConstructionKey, Construction>();
        }

        public Expression GetNewObjectCreation(IObjectMappingData mappingData)
        {
            var objectCreation = _constructorsCache.GetOrAdd(new ConstructionKey(mappingData), key =>
            {
                var constructions = new List<Construction>();

                bool newingConstructorRequired;

                AddConfiguredConstructions(
                    constructions,
                    key,
                    out newingConstructorRequired);

                if (newingConstructorRequired)
                {
                    AddNewingConstruction(constructions, key);
                }

                if (constructions.None())
                {
                    key.MappingData = null;
                    return null;
                }

                var compositeConstruction = new Construction(constructions, key);

                key.MappingData = null;

                return compositeConstruction;
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

            var greediestAvailableConstructor = mapperData.InstanceVariable.Type
                .GetPublicInstanceConstructors()
                .Select(ctor => CreateConstructorData(ctor, key))
                .Where(ctor => ctor.CanBeConstructed)
                .OrderByDescending(ctor => ctor.NumberOfParameters)
                .FirstOrDefault();

            if (greediestAvailableConstructor == null)
            {
                return;
            }

            foreach (var memberAndDataSourceSet in greediestAvailableConstructor.ArgumentDataSources)
            {
                key.MappingData.MapperData.RegisterTargetMemberDataSourcesIfRequired(
                    memberAndDataSourceSet.Item1,
                    memberAndDataSourceSet.Item2);
            }

            constructions.Add(greediestAvailableConstructor.Construction);
        }

        private static ConstructorData CreateConstructorData(ConstructorInfo ctor, ConstructionKey key)
        {
            var mapperData = key.MappingData.MapperData;

            var ctorData = new ConstructorData(
                ctor,
                ctor.GetParameters()
                    .Select(p =>
                    {
                        var parameterMapperData = new ChildMemberMapperData(
                            mapperData.TargetMember.Append(Member.ConstructorParameter(p)),
                            mapperData);

                        return key.MappingData.GetChildMappingData(parameterMapperData);
                    })
                    .Select(memberMappingData =>
                    {
                        var dataSources = mapperData
                            .MapperContext
                            .DataSources
                            .FindFor(memberMappingData);

                        return Tuple.Create(memberMappingData.MapperData.TargetMember, dataSources);
                    })
                    .ToArray());

            return ctorData;
        }

        public void Reset() => _constructorsCache.Empty();

        #region Helper Classes

        private class ConstructionKey
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

            public IObjectMappingData MappingData { get; set; }

            public override bool Equals(object obj)
            {
                var otherKey = (ConstructionKey)obj;

                // ReSharper disable once PossibleNullReferenceException
                return (otherKey._ruleSet == _ruleSet) &&
                    (otherKey._sourceMember == _sourceMember) &&
                    (otherKey._targetMember == _targetMember);
            }

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
                    argumentValues.Add(argumentDataSource.Item2.GetValueExpression());
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
            private readonly ParameterExpression _mappingDataObject;

            public Construction(ICollection<Construction> constructions, ConstructionKey key)
                : this(constructions.ReverseChain())
            {
                UsesMappingDataObjectParameter = constructions.Any(c => c.UsesMappingDataObjectParameter);
                _mappingDataObject = key.MappingData.MapperData.MappingDataObject;
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