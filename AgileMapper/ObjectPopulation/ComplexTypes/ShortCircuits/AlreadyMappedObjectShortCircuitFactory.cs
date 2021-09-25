namespace AgileObjects.AgileMapper.ObjectPopulation.ComplexTypes.ShortCircuits
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;

    internal static class AlreadyMappedObjectShortCircuitFactory
    {
        public static Expression GetShortCircuitOrNull(IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            if (!ObjectCouldHaveBeenMappedBefore(mapperData))
            {
                return null;
            }

            var tryGetMethod = MappingExecutionContextConstants.TryGetMethod
                .MakeGenericMethod(mapperData.SourceType, mapperData.TargetType);

            var tryGetCall = Expression.Call(
                Constants.ExecutionContextParameter,
                tryGetMethod,
                mapperData.SourceObject,
                mapperData.TargetInstance);

            var ifTryGetReturn = Expression.IfThen(
                tryGetCall,
                mapperData.GetReturnExpression(mapperData.TargetInstance));

            return ifTryGetReturn;
        }

        private static bool ObjectCouldHaveBeenMappedBefore(ObjectMapperData mapperData)
        {
            return mapperData.RuleSet.Settings.AllowObjectTracking &&
                   mapperData.CacheMappedObjects &&
                   mapperData.TargetTypeHasBeenMappedBefore &&
                  !mapperData.SourceType.IsDictionary();
        }
    }
}