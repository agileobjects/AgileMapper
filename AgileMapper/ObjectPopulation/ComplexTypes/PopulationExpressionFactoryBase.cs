namespace AgileObjects.AgileMapper.ObjectPopulation.ComplexTypes
{
    using System.Collections.Generic;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions.Internal;
    using Members;
    using Members.Population;
    using static InvocationPosition;

    internal abstract class PopulationExpressionFactoryBase
    {
        public void AddPopulation(MappingCreationContext context)
        {
            var mappingData = context.MappingData;
            var mapperData = mappingData.MapperData;

            GetCreationCallbacks(context, out var preCreationCallback, out var postCreationCallback);

            var populationsAndCallbacks = GetPopulationsAndCallbacks(mappingData).ToList();
            var guardPopulations = false;

            if (context.InstantiateLocalTargetVariable && mapperData.Context.UseLocalTargetVariable)
            {
                context.MappingExpressions.AddUnlessNullOrEmpty(preCreationCallback);

                var hasPostCreationCallback = postCreationCallback != null;
                var assignCreatedObject = hasPostCreationCallback;

                var localTargetVariableInstantiation = GetLocalTargetVariableInstantiation(
                    assignCreatedObject,
                    populationsAndCallbacks,
                    mappingData);

                context.MappingExpressions.Add(localTargetVariableInstantiation);

                guardPopulations = LocalVariableCouldBeNull(localTargetVariableInstantiation);

                if (hasPostCreationCallback)
                {
                    context.MappingExpressions.Add(postCreationCallback);
                }
            }

            if (IncludeObjectRegistration(mapperData))
            {
                context.MappingExpressions.Add(GetObjectRegistrationCall(mapperData));
            }

            if (populationsAndCallbacks.None())
            {
                goto AddTypeTester;
            }

            if (guardPopulations)
            {
                context.MappingExpressions.Add(Expression.IfThen(
                    mappingData.MapperData.LocalTargetVariable.GetIsNotDefaultComparison(),
                    Expression.Block(populationsAndCallbacks)));

                goto AddTypeTester;
            }

            context.MappingExpressions.AddRange(populationsAndCallbacks);

            AddTypeTester:
            mappingData.MapperKey.AddSourceMemberTypeTesterIfRequired(mappingData);
        }

        private static void GetCreationCallbacks(
            MappingCreationContext context,
            out Expression preCreationCallback,
            out Expression postCreationCallback)
        {
            if (context.RuleSet.Settings.UseSingleRootMappingExpression ||
               !context.InstantiateLocalTargetVariable ||
                context.MapperData.TargetIsDefinitelyPopulated())
            {
                preCreationCallback = postCreationCallback = null;
                return;
            }

            preCreationCallback = GetCreationCallbackOrNull(Before, context.MapperData);
            postCreationCallback = GetCreationCallbackOrNull(After, context.MapperData);
        }

        private static Expression GetCreationCallbackOrNull(InvocationPosition invocationPosition, IMemberMapperData mapperData)
            => mapperData.MapperContext.UserConfigurations.GetCreationCallbackOrNull(invocationPosition, mapperData);

        private IEnumerable<Expression> GetPopulationsAndCallbacks(IObjectMappingData mappingData)
        {
            foreach (var memberPopulator in MemberPopulatorFactory.Default.Create(mappingData))
            {
                if (!memberPopulator.CanPopulate)
                {
                    yield return memberPopulator.GetPopulation();
                    continue;
                }

                foreach (var expression in GetPopulationExpressionsFor(memberPopulator, mappingData))
                {
                    yield return expression;
                }
            }
        }

        protected abstract IEnumerable<Expression> GetPopulationExpressionsFor(
            IMemberPopulator memberPopulator,
            IObjectMappingData mappingData);

        private Expression GetLocalTargetVariableInstantiation(
            bool assignCreatedObject,
            IList<Expression> memberPopulations,
            IObjectMappingData mappingData)
        {
            var localTargetVariableValue = TargetObjectResolutionFactory.GetObjectResolution(
                GetTargetObjectCreation,
                mappingData,
                memberPopulations,
                assignCreatedObject);

            return mappingData.MapperData.LocalTargetVariable.AssignTo(localTargetVariableValue);
        }

        protected virtual Expression GetTargetObjectCreation(
            IObjectMappingData mappingData,
            IList<Expression> memberPopulations)
        {
            return mappingData
                .MapperData
                .MapperContext
                .ConstructionFactory
                .GetTargetObjectCreation(mappingData);
        }

        private static bool LocalVariableCouldBeNull(Expression instantiation)
        {
            if (instantiation.Type.CannotBeNull())
            {
                return false;
            }

            while (true)
            {
                switch (instantiation.NodeType)
                {
                    case ExpressionType.Assign:
                    case ExpressionType.Coalesce:
                        var binary = (BinaryExpression)instantiation;
                        instantiation = binary.Right;
                        continue;

                    case ExpressionType.Call:
                        return true;

                    default:
                        return false;
                }
            }
        }

        #region Object Registration

        private static bool IncludeObjectRegistration(ObjectMapperData mapperData)
        {
            return mapperData.CacheMappedObjects &&
                   mapperData.RuleSet.Settings.AllowObjectTracking &&
                  !mapperData.TargetTypeWillNotBeMappedAgain;
        }

        private static Expression GetObjectRegistrationCall(ObjectMapperData mapperData)
        {
            var registerMethod = MappingExecutionContextConstants.RegisterMethod
                .MakeGenericMethod(mapperData.SourceType, mapperData.TargetType);

            return Expression.Call(
                Constants.ExecutionContextParameter,
                registerMethod,
                mapperData.SourceObject,
                mapperData.TargetInstance);
        }

        #endregion
    }
}