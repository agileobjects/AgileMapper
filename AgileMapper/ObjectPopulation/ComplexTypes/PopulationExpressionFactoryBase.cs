namespace AgileObjects.AgileMapper.ObjectPopulation.ComplexTypes
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions.Internal;
    using Members;
    using Members.Population;
    using NetStandardPolyfills;
    using static CallbackPosition;

    internal abstract class PopulationExpressionFactoryBase
    {
        public IEnumerable<Expression> GetPopulation(IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            GetCreationCallbacks(mapperData, out var preCreationCallback, out var postCreationCallback);

            var populationsAndCallbacks = GetPopulationsAndCallbacks(mappingData).ToList();

            yield return preCreationCallback;

            if (mapperData.Context.UseLocalVariable)
            {
                var assignCreatedObject = postCreationCallback != null;

                yield return GetLocalVariableInstantiation(assignCreatedObject, populationsAndCallbacks, mappingData);
            }

            yield return postCreationCallback;

            yield return GetObjectRegistrationCallOrNull(mapperData);

            foreach (var population in populationsAndCallbacks)
            {
                yield return population;
            }

            mappingData.MapperKey.AddSourceMemberTypeTesterIfRequired(mappingData);
        }

        private static void GetCreationCallbacks(
            IMemberMapperData mapperData,
            out Expression preCreationCallback,
            out Expression postCreationCallback)
        {
            if (mapperData.RuleSet.Settings.UseSingleRootMappingExpression ||
                mapperData.TargetIsDefinitelyPopulated())
            {
                preCreationCallback = postCreationCallback = null;
                return;
            }

            preCreationCallback = GetCreationCallbackOrNull(Before, mapperData);
            postCreationCallback = GetCreationCallbackOrNull(After, mapperData);
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

            var localVariableAssignment = mappingData.MapperData.LocalVariable.AssignWith(localVariableValue);

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

            var mappedObjectsCache = mapperData.GetMappedObjectsCache();

            return Expression.Call(
                mappedObjectsCache,
                mappedObjectsCache.Type.GetPublicInstanceMethod("Register"),
                mapperData.SourceObject,
                mapperData.TargetInstance);
        }

        #endregion
    }
}