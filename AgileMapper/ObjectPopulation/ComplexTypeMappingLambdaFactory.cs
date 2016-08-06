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

        protected override bool IsNotConstructable(IObjectMappingContext omc)
            => GetNewObjectCreation(omc) == null;

        protected override IEnumerable<Expression> GetShortCircuitReturns(GotoExpression returnNull, IObjectMappingContext omc)
        {
            yield return GetStrategyShortCircuitReturns(returnNull, omc);
            yield return GetExistingObjectShortCircuit(returnNull.Target, omc);
        }

        private static Expression GetStrategyShortCircuitReturns(Expression returnNull, IMemberMappingContext context)
        {
            if (!context.SourceMember.Matches(context.TargetMember))
            {
                return Constants.EmptyExpression;
            }

            var shortCircuitConditions = context.MappingContext
                .RuleSet
                .ComplexTypeMappingShortCircuitStrategy
                .GetConditions(context)
                .Select(condition => (Expression)Expression.IfThen(condition, returnNull));

            var shortCircuitBlock = Expression.Block(shortCircuitConditions);

            return shortCircuitBlock;
        }

        private static Expression GetExistingObjectShortCircuit(LabelTarget returnTarget, IMemberMappingContext context)
        {
            var tryGetCall = Expression.Call(
                Expression.Property(context.Parameter, "MappingContext"),
                MappingContext.TryGetMethod.MakeGenericMethod(context.SourceType, context.InstanceVariable.Type),
                context.SourceObject,
                context.InstanceVariable);

            var ifTryGetReturn = Expression.IfThen(
                tryGetCall,
                Expression.Return(returnTarget, context.InstanceVariable));

            return ifTryGetReturn;
        }

        protected override IEnumerable<Expression> GetObjectPopulation(IObjectMappingContext omc)
        {
            yield return GetCreationCallback(CallbackPosition.Before, omc);

            var instanceVariableValue = GetObjectResolution(omc);
            var instanceVariableAssignment = Expression.Assign(omc.InstanceVariable, instanceVariableValue);
            yield return instanceVariableAssignment;

            yield return GetCreationCallback(CallbackPosition.After, omc);

            yield return GetObjectRegistrationCall(omc);

            var memberPopulations = MemberPopulationFactory
                .Create(omc)
                .Select(p => p.IsSuccessful ? GetPopulationWithCallbacks(p, omc) : p.GetPopulation());

            foreach (var population in memberPopulations)
            {
                yield return population;
            }
        }

        private static Expression GetCreationCallback(CallbackPosition callbackPosition, IMemberMappingContext context)
            => GetCallbackOrEmpty(c => c.GetCreationCallbackOrNull(callbackPosition, context), context);

        private static Expression GetObjectResolution(IObjectMappingContext omc)
        {
            var createdObjectAssignment = Expression.Assign(omc.CreatedObject, GetNewObjectCreation(omc));
            var contextTargetAssignment = Expression.Assign(omc.TargetObject, createdObjectAssignment);
            var existingOrCreatedObject = Expression.Coalesce(omc.TargetObject, contextTargetAssignment);

            return existingOrCreatedObject;
        }

        private static Expression GetNewObjectCreation(IObjectMappingContext omc)
        {
            var objectCreationKey = string.Format(
                CultureInfo.InvariantCulture,
                "{0} -> {1}: {2} Ctor",
                omc.SourceMember.Signature,
                omc.TargetMember.Signature,
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
                        configuredFactory.GetConditionOrNull(omc));

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

        private static Expression GetObjectRegistrationCall(IMemberMappingContext context)
        {
            return Expression.Call(
                Expression.Property(context.Parameter, "MappingContext"),
                MappingContext.RegisterMethod.MakeGenericMethod(context.SourceType, context.TargetType),
                context.SourceObject,
                context.InstanceVariable);
        }

        private static Expression GetPopulationWithCallbacks(IMemberPopulation memberPopulation, IMemberMappingContext parentContext)
        {
            var prePopulationCallback = GetCallbackOrEmpty(
                c => c.GetCallbackOrNull(CallbackPosition.Before, memberPopulation.Context, parentContext),
                parentContext);

            var population = memberPopulation.GetPopulation();

            var postPopulationCallback = GetCallbackOrEmpty(
                c => c.GetCallbackOrNull(CallbackPosition.After, memberPopulation.Context, parentContext),
                parentContext);

            if ((prePopulationCallback == Constants.EmptyExpression) &&
                (postPopulationCallback == Constants.EmptyExpression))
            {
                return population;
            }

            return Expression.Block(prePopulationCallback, population, postPopulationCallback);
        }

        protected override Expression GetReturnValue(IObjectMappingContext omc) => omc.InstanceVariable;

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