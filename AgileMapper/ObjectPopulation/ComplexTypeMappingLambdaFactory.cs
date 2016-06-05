namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using DataSources;
    using Extensions;
    using Members;

    internal class ComplexTypeMappingLambdaFactory<TSource, TTarget, TInstance>
        : ObjectMappingLambdaFactoryBase<TSource, TTarget, TInstance>
    {
        public static readonly ObjectMappingLambdaFactoryBase<TSource, TTarget, TInstance> Instance =
            new ComplexTypeMappingLambdaFactory<TSource, TTarget, TInstance>();

        protected override bool IsNotConstructable(IObjectMappingContext omc)
            => GetNewObjectCreation(omc) == null;

        protected override IEnumerable<Expression> GetShortCircuitReturns(GotoExpression returnNull, IObjectMappingContext omc)
        {
            yield return GetStrategyShortCircuitReturns(returnNull, omc);
            yield return GetExistingObjectShortCircuit(returnNull.Target, omc);
        }

        private static Expression GetStrategyShortCircuitReturns(Expression returnNull, IObjectMappingContext omc)
        {
            var matchingSourceMemberDataSource = omc
                .MapperContext
                .DataSources
                .GetSourceMemberDataSourceOrNull(omc);

            if (matchingSourceMemberDataSource == null)
            {
                return Constants.EmptyExpression;
            }

            Expression sourceObject;
            Func<IEnumerable<Expression>, Expression> blockBuilder;

            if (matchingSourceMemberDataSource.Value == omc.SourceObject)
            {
                sourceObject = omc.SourceObject;
                blockBuilder = Expression.Block;
            }
            else
            {
                sourceObject = Expression.Variable(matchingSourceMemberDataSource.Value.Type, "matchingSource");
                Expression assignSourceObject = Expression.Assign(sourceObject, matchingSourceMemberDataSource.Value);

                blockBuilder = conditions => Expression.Block(
                    new[] { (ParameterExpression)sourceObject },
                    new[] { assignSourceObject }.Concat(conditions));
            }

            var shortCircuitConditions = omc.MappingContext
                .RuleSet
                .ComplexTypeMappingShortCircuitStrategy
                .GetConditions(sourceObject, omc)
                .Select(condition => Expression.IfThen(condition, returnNull))
                .ToArray();

            var shortCircuitBlock = blockBuilder.Invoke(shortCircuitConditions);

            return shortCircuitBlock;
        }

        private static Expression GetExistingObjectShortCircuit(LabelTarget returnTarget, IObjectMappingContext omc)
        {
            var ifTryGetReturn = Expression.IfThen(
                omc.TryGetCall,
                Expression.Return(returnTarget, omc.InstanceVariable));

            return ifTryGetReturn;
        }

        protected override Expression GetObjectResolution(IObjectMappingContext omc)
        {
            var createdInstance = Expression.Coalesce(omc.ExistingObject, GetNewObjectCreation(omc));

            return Expression.Assign(omc.Object, createdInstance);
        }

        private static Expression GetNewObjectCreation(IObjectMappingContext omc)
        {
            var objectCreationKey = string.Format(
                CultureInfo.InvariantCulture,
                "{0} -> {1}, {2}: {3} Ctor",
                omc.SourceType.FullName,
                omc.TargetType.FullName,
                omc.InstanceVariable.Type.FullName,
                omc.RuleSetName);

            return omc.MapperContext.Cache.GetOrAdd(objectCreationKey, k =>
            {
                var constructions = new List<Construction>();
                var newingConstructorRequired = true;

                var configuredFactories = omc
                    .MapperContext
                    .UserConfigurations
                    .GetObjectFactories(omc);

                foreach (var configuredFactory in configuredFactories)
                {
                    var configuredConstruction = new Construction(
                        configuredFactory.Create(omc),
                        configuredFactory.GetCondition(omc));

                    constructions.Insert(0, configuredConstruction);

                    if (!configuredFactory.HasConfiguredCondition)
                    {
                        newingConstructorRequired = false;
                        break;
                    }
                }

                if (newingConstructorRequired)
                {
                    var greediestAvailableConstructor = omc.InstanceVariable.Type
                        .GetConstructors(Constants.PublicInstance)
                        .Select(ctor => new ConstructorData(
                            ctor,
                            ctor.GetParameters()
                                .Select(p => new MemberMappingContext(
                                    omc.TargetMember.Append(Member.ConstructorParameter(p)),
                                    omc))
                                .Select(context => omc
                                    .MapperContext
                                    .DataSources
                                    .FindFor(context))
                                .ToArray()))
                        .Where(ctor => ctor.CanBeConstructed)
                        .OrderByDescending(ctor => ctor.NumberOfParameters)
                        .FirstOrDefault();

                    if (greediestAvailableConstructor != null)
                    {
                        constructions.Insert(0, greediestAvailableConstructor.Construction);
                    }
                }

                if (constructions.None())
                {
                    return null;
                }

                return constructions
                    .Skip(1)
                    .Aggregate(
                        constructions.First().Expression,
                        (constructionSoFar, construction) =>
                            Expression.Condition(construction.Condition, construction.Expression, constructionSoFar));
            });
        }

        protected override IEnumerable<Expression> GetObjectPopulation(Expression instanceVariableValue, IObjectMappingContext omc)
        {
            var objectRegistration = omc.ObjectRegistrationCall;
            var objectCreationCallback = GetObjectCreatedCallback(omc);
            var memberPopulations = MemberPopulationFactory.Create(omc);

            var successfulPopulations = memberPopulations
                .Where(p => p.IsSuccessful)
                .Select(p => p.GetPopulation())
                .ToArray();

            var unsuccessfulPopulations = memberPopulations
                .Where(p => !p.IsSuccessful)
                .Select(p => p.GetPopulation())
                .ToArray();

            return Enumerable.Concat(new[] { objectRegistration, objectCreationCallback }
                    .Concat(successfulPopulations), unsuccessfulPopulations)
                .ToArray();
        }

        private static Expression GetObjectCreatedCallback(IObjectMappingContext omc)
        {
            var callback = omc
                .MapperContext
                .UserConfigurations
                .GetCreationCallbackOrNull(omc);

            return (callback != null) ? callback.IntegrateCallback(omc) : Constants.EmptyExpression;
        }

        protected override Expression GetReturnValue(Expression instanceVariableValue, IObjectMappingContext omc)
            => omc.InstanceVariable;

        private class ConstructorData
        {
            public ConstructorData(ConstructorInfo constructor, ICollection<DataSourceSet> argumentDataSources)
            {
                CanBeConstructed = argumentDataSources.All(ds => ds.HasValue);
                NumberOfParameters = argumentDataSources.Count;

                if (CanBeConstructed)
                {
                    Construction = new Construction(
                        Expression.New(constructor, argumentDataSources.Select(ds => ds.Value)));
                }
            }

            public bool CanBeConstructed { get; }

            public int NumberOfParameters { get; }

            public Construction Construction { get; }
        }

        private class Construction
        {
            public Construction(Expression construction, Expression condition = null)
            {
                Expression = construction;
                Condition = condition;
            }

            public Expression Expression { get; }

            public Expression Condition { get; }
        }
    }
}