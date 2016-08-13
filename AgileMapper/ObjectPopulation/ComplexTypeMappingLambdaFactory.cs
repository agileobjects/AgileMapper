namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using DataSources;
    using Extensions;
    using Members;

    internal class ComplexTypeMappingLambdaFactory : ObjectMappingLambdaFactoryBase
    {
        public static readonly ObjectMappingLambdaFactoryBase Instance = new ComplexTypeMappingLambdaFactory();

        protected override bool IsNotConstructable(IObjectMapperCreationData data)
            => GetNewObjectCreation(data) == null;

        protected override IEnumerable<Expression> GetShortCircuitReturns(GotoExpression returnNull, ObjectMapperData data)
        {
            yield return GetStrategyShortCircuitReturns(returnNull, data);
            yield return GetExistingObjectShortCircuit(returnNull.Target, data);
        }

        private static Expression GetStrategyShortCircuitReturns(Expression returnNull, MemberMapperData data)
        {
            if (!data.SourceMember.Matches(data.TargetMember))
            {
                return Constants.EmptyExpression;
            }

            var shortCircuitConditions = data
                .RuleSet
                .ComplexTypeMappingShortCircuitStrategy
                .GetConditions(data)
                .Select(condition => (Expression)Expression.IfThen(condition, returnNull));

            var shortCircuitBlock = Expression.Block(shortCircuitConditions);

            return shortCircuitBlock;
        }

        private static Expression GetExistingObjectShortCircuit(LabelTarget returnTarget, MemberMapperData data)
        {
            var tryGetCall = Expression.Call(
                Expression.Property(data.Parameter, "MappingContext"),
                MappingContext.TryGetMethod.MakeGenericMethod(data.SourceType, data.InstanceVariable.Type),
                data.SourceObject,
                data.InstanceVariable);

            var ifTryGetReturn = Expression.IfThen(
                tryGetCall,
                Expression.Return(returnTarget, data.InstanceVariable));

            return ifTryGetReturn;
        }

        protected override IEnumerable<Expression> GetObjectPopulation(IObjectMapperCreationData data)
        {
            var mapperData = data.MapperData;

            yield return GetCreationCallback(CallbackPosition.Before, mapperData);

            var instanceVariableValue = GetObjectResolution(data);
            var instanceVariableAssignment = Expression.Assign(mapperData.InstanceVariable, instanceVariableValue);
            yield return instanceVariableAssignment;

            yield return GetCreationCallback(CallbackPosition.After, mapperData);

            yield return GetObjectRegistrationCall(mapperData);

            var memberPopulations = MemberPopulationFactory
                .Create(data)
                .Select(p => p.IsSuccessful ? GetPopulationWithCallbacks(p, mapperData) : p.GetPopulation());

            foreach (var population in memberPopulations)
            {
                yield return population;
            }
        }

        private static Expression GetCreationCallback(CallbackPosition callbackPosition, MemberMapperData data)
            => GetCallbackOrEmpty(c => c.GetCreationCallbackOrNull(callbackPosition, data), data);

        private static Expression GetObjectResolution(IObjectMapperCreationData data)
        {
            var mapperData = data.MapperData;

            var createdObjectAssignment = Expression.Assign(mapperData.CreatedObject, GetNewObjectCreation(data));
            var instanceDataTargetAssignment = Expression.Assign(mapperData.TargetObject, createdObjectAssignment);
            var existingOrCreatedObject = Expression.Coalesce(mapperData.TargetObject, instanceDataTargetAssignment);

            return existingOrCreatedObject;
        }

        private static Expression GetNewObjectCreation(IObjectMapperCreationData data)
        {
            var objectCreationKey = string.Format(
                CultureInfo.InvariantCulture,
                "{0} -> {1}: {2} Ctor",
                data.SourceMember.Signature,
                data.TargetMember.Signature,
                data.RuleSet.Name);

            return data.MapperData.MapperContext.Cache.GetOrAdd(objectCreationKey, k =>
            {
                var mapperData = data.MapperData;

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

                    if (!configuredFactory.HasConfiguredCondition)
                    {
                        newingConstructorRequired = false;
                        break;
                    }
                }

                if (newingConstructorRequired)
                {
                    var greediestAvailableConstructor = mapperData.InstanceVariable.Type
                        .GetConstructors(Constants.PublicInstance)
                        .Select(ctor => new ConstructorData(
                            ctor,
                            ctor.GetParameters()
                                .Select(p =>
                                {
                                    var childMapperData = new MemberMapperData(
                                        data.TargetMember.Append(Member.ConstructorParameter(p)),
                                        mapperData);

                                    return data.GetChildCreationData(childMapperData);
                                })
                                .Select(memberData => mapperData
                                    .MapperContext
                                    .DataSources
                                    .FindFor(memberData))
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

        private static Expression GetObjectRegistrationCall(MemberMapperData data)
        {
            return Expression.Call(
                Expression.Property(data.Parameter, "MappingContext"),
                MappingContext.RegisterMethod.MakeGenericMethod(data.SourceType, data.TargetType),
                data.SourceObject,
                data.InstanceVariable);
        }

        private static Expression GetPopulationWithCallbacks(IMemberPopulation memberPopulation, MemberMapperData parentData)
        {
            var prePopulationCallback = GetCallbackOrEmpty(
                c => c.GetCallbackOrNull(CallbackPosition.Before, memberPopulation.MapperData, parentData),
                parentData);

            var population = memberPopulation.GetPopulation();

            var postPopulationCallback = GetCallbackOrEmpty(
                c => c.GetCallbackOrNull(CallbackPosition.After, memberPopulation.MapperData, parentData),
                parentData);

            if ((prePopulationCallback == Constants.EmptyExpression) &&
                (postPopulationCallback == Constants.EmptyExpression))
            {
                return population;
            }

            return Expression.Block(prePopulationCallback, population, postPopulationCallback);
        }

        protected override Expression GetReturnValue(ObjectMapperData data) => data.InstanceVariable;

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