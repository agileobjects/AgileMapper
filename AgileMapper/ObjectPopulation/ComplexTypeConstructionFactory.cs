namespace AgileObjects.AgileMapper.ObjectPopulation
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
                var mapperData = key.MappingData.MapperData;

                var constructions = new List<Construction>();
                var newingConstructorRequired = true;

                var configuredFactories = mapperData
                    .MapperContext
                    .UserConfigurations
                    .GetObjectFactories(mapperData);

                foreach (var configuredFactory in configuredFactories)
                {
                    var configuredConstruction = new Construction(
                        configuredFactory.Create(mapperData),
                        configuredFactory.GetConditionOrNull(mapperData));

                    constructions.Insert(0, configuredConstruction);

                    if (configuredConstruction.Condition == null)
                    {
                        newingConstructorRequired = false;
                        break;
                    }
                }

                if (newingConstructorRequired)
                {
                    var greediestAvailableConstructor = mapperData.InstanceVariable.Type
                        .GetPublicInstanceConstructors()
                        .Select(ctor => new ConstructorData(
                            ctor,
                            ctor.GetParameters()
                                .Select(p =>
                                {
                                    var parameterMapperData = new ChildMemberMapperData(
                                        mapperData.TargetMember.Append(Member.ConstructorParameter(p)),
                                        mapperData);

                                    return key.MappingData.GetChildMappingData(parameterMapperData);
                                })
                                .Select(memberData =>
                                {
                                    var dataSources = mapperData
                                        .MapperContext
                                        .DataSources
                                        .FindFor(memberData);

                                    return Tuple.Create(memberData.MapperData.TargetMember, dataSources);
                                })
                                .ToArray()))
                        .Where(ctor => ctor.CanBeConstructed)
                        .OrderByDescending(ctor => ctor.NumberOfParameters)
                        .FirstOrDefault();

                    if (greediestAvailableConstructor != null)
                    {
                        foreach (var memberAndDataSourceSet in greediestAvailableConstructor.ArgumentDataSources)
                        {
                            key.MappingData.MapperData.RegisterTargetMemberDataSourcesIfRequired(
                                memberAndDataSourceSet.Item1,
                                memberAndDataSourceSet.Item2);
                        }

                        constructions.Insert(0, greediestAvailableConstructor.Construction);
                    }
                }

                if (constructions.None())
                {
                    key.MappingData = null;
                    return null;
                }

                var constructionExpression = constructions
                    .Skip(1)
                    .Aggregate(
                        constructions.First().Expression,
                        (constructionSoFar, construction) =>
                            Expression.Condition(construction.Condition, construction.Expression, constructionSoFar));

                var compositeConstruction = new Construction(constructionExpression, key);

                key.MappingData = null;

                return compositeConstruction;
            });

            var creationExpression = objectCreation?.GetConstruction(mappingData);

            return creationExpression;
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
                    argumentValues.Add(argumentDataSource.Item2.Value);
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

        private class Construction
        {
            private readonly ParameterExpression _mappingDataObject;

            public Construction(Expression construction, ConstructionKey key)
                : this(construction)
            {
                _mappingDataObject = key.MappingData.MapperData.MappingDataObject;
            }

            public Construction(Expression construction, Expression condition = null)
            {
                Expression = construction;
                Condition = condition;
            }

            public Expression Expression { get; }

            public Expression Condition { get; }

            public Expression GetConstruction(IObjectMappingData mappingData)
                => Expression.Replace(_mappingDataObject, mappingData.MapperData.MappingDataObject);
        }

        #endregion
    }
}