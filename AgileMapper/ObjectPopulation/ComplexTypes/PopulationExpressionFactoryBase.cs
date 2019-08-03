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
    using NetStandardPolyfills;
    using static CallbackPosition;

    internal abstract class PopulationExpressionFactoryBase
    {
        public IEnumerable<Expression> GetPopulation(MappingCreationContext context)
        {
            var mappingData = context.MappingData;
            var mapperData = mappingData.MapperData;

            GetCreationCallbacks(context, out var preCreationCallback, out var postCreationCallback);

            var populationsAndCallbacks = GetPopulationsAndCallbacks(mappingData).ToList();

            if (context.InstantiateLocalVariable && mapperData.Context.UseLocalVariable)
            {
                if (preCreationCallback != null)
                {
                    yield return preCreationCallback;
                }
                
                var hasPostCreationCallback = postCreationCallback != null;
                var assignCreatedObject = hasPostCreationCallback;

                yield return GetLocalVariableInstantiation(assignCreatedObject, populationsAndCallbacks, mappingData);

                if (hasPostCreationCallback)
                {
                    yield return postCreationCallback;
                }
            }

            if (IncludeObjectRegistration(mapperData))
            {
                yield return GetObjectRegistrationCall(mapperData);
            }

            foreach (var population in populationsAndCallbacks)
            {
                yield return population;
            }

            mappingData.MapperKey.AddSourceMemberTypeTesterIfRequired(mappingData);
        }

        private static void GetCreationCallbacks(
            MappingCreationContext context,
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
                GetTargetObjectCreation,
                mappingData,
                memberPopulations,
                assignCreatedObject);

            return mappingData.MapperData.LocalVariable.AssignTo(localVariableValue);
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

        #region Object Registration

        private static bool IncludeObjectRegistration(ObjectMapperData mapperData)
        {
            return mapperData.CacheMappedObjects &&
                   mapperData.RuleSet.Settings.AllowObjectTracking &&
                  !mapperData.TargetTypeWillNotBeMappedAgain;
        }

        private static Expression GetObjectRegistrationCall(ObjectMapperData mapperData)
        {
            var registerMethod = typeof(IObjectMappingDataUntyped)
                .GetPublicInstanceMethod(nameof(IObjectMappingDataUntyped.Register))
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