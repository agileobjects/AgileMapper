namespace AgileObjects.AgileMapper.ObjectPopulation.ComplexTypes
{
    using System.Collections.Generic;
    using System.Linq;
    using Extensions.Internal;
    using Members;
    using Members.Population;
    using NetStandardPolyfills;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using static CallbackPosition;

    internal abstract class PopulationExpressionFactoryBase
    {
        public IEnumerable<Expression> GetPopulation(MappingExpressionFactoryBase.MappingCreationContext context)
        {
            var mappingData = context.MappingData;
            var mapperData = context.MapperData;

            GetCreationCallbacks(context, out var preCreationCallback, out var postCreationCallback);

            var populationsAndCallbacks = GetPopulationsAndCallbacks(mappingData).ToList();

            if (context.InstantiateLocalVariable && mapperData.Context.UseLocalVariable)
            {
                yield return preCreationCallback;

                var assignCreatedObject = postCreationCallback != null;

                yield return GetLocalVariableInstantiation(assignCreatedObject, populationsAndCallbacks, mappingData);

                yield return postCreationCallback;
            }

            yield return GetObjectRegistrationCallOrNull(mapperData);

            foreach (var population in populationsAndCallbacks)
            {
                yield return population;
            }

            mappingData.MapperKey.AddSourceMemberTypeTesterIfRequired(mappingData);
        }

        private static void GetCreationCallbacks(
            MappingExpressionFactoryBase.MappingCreationContext context,
            out Expression preCreationCallback,
            out Expression postCreationCallback)
        {
            if (context.RuleSet.Settings.UseSingleRootMappingExpression ||
               !context.InstantiateLocalVariable ||
                context.MapperData.TargetIsDefinitelyPopulated())
            {
                preCreationCallback = postCreationCallback = null;
                return;
            }

            preCreationCallback = GetCreationCallbackOrNull(Before, context.MapperData);
            postCreationCallback = GetCreationCallbackOrNull(After, context.MapperData);
        }

        private static Expression GetCreationCallbackOrNull(CallbackPosition callbackPosition, IMemberMapperData mapperData)
            => mapperData.MapperContext.UserConfigurations.GetCreationCallbackOrNull(callbackPosition, mapperData);

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

        private Expression GetLocalVariableInstantiation(
            bool assignCreatedObject,
            IList<Expression> memberPopulations,
            IObjectMappingData mappingData)
        {
            var localVariableValue = TargetObjectResolutionFactory.GetObjectResolution(
                GetNewObjectCreation,
                mappingData,
                memberPopulations,
                assignCreatedObject);

            var localVariableAssignment = mappingData.MapperData.LocalVariable.AssignTo(localVariableValue);

            return localVariableAssignment;
        }

        protected virtual Expression GetNewObjectCreation(
            IObjectMappingData mappingData,
            IList<Expression> memberPopulations)
        {
            return mappingData
                .MapperData
                .MapperContext
                .ConstructionFactory
                .GetNewObjectCreation(mappingData);
        }

        #region Object Registration

        private static Expression GetObjectRegistrationCallOrNull(ObjectMapperData mapperData)
        {
            if (!mapperData.RuleSet.Settings.AllowObjectTracking ||
                !mapperData.CacheMappedObjects ||
                 mapperData.TargetTypeWillNotBeMappedAgain)
            {
                return null;
            }

            var registerMethod = typeof(IObjectMappingDataUntyped)
                .GetPublicInstanceMethod("Register")
                .MakeGenericMethod(mapperData.SourceType, mapperData.TargetType);

            return Expression.Call(
                mapperData.EntryPointMapperData.MappingDataObject,
                registerMethod,
                mapperData.SourceObject,
                mapperData.TargetInstance);
        }

        #endregion
    }
}