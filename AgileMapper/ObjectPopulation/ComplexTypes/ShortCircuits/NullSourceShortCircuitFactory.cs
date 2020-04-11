namespace AgileObjects.AgileMapper.ObjectPopulation.ComplexTypes.ShortCircuits
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;

    internal static class NullSourceShortCircuitFactory
    {
        public static Expression GetShortCircuitOrNull(IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            if (SourceCannotBeNull(mapperData))
            {
                return null;
            }

            var returnNull = Expression.Return(
                mapperData.ReturnLabelTarget,
                mapperData.GetTargetTypeDefault());

            return Expression.IfThen(
                mapperData.SourceObject.GetIsDefaultComparison(),
                returnNull);
        }

        private static bool SourceCannotBeNull(IMemberMapperData mapperData)
        {
            if (mapperData.Context.IsForDerivedType)
            {
                return true;
            }

            if (mapperData.SourceType.IsValueType())
            {
                return true;
            }

            if (mapperData.RuleSet.Settings.SourceElementsCouldBeNull &&
                mapperData.TargetMemberIsEnumerableElement())
            {
                return mapperData.HasSameSourceAsParent();
            }

            return true;
        }
    }
}