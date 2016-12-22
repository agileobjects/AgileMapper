namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;
    using Members;

    internal static class DerivedMappingFactory
    {
        public static Expression GetDerivedTypeMapping(
            IObjectMappingData declaredTypeMappingData,
            Expression sourceValue,
            Type targetType)
        {
            var declaredTypeMapperData = declaredTypeMappingData.MapperData;

            var targetValue = declaredTypeMapperData.TargetMember.IsReadable
                ? declaredTypeMapperData.TargetObject.GetConversionTo(targetType)
                : targetType.ToDefaultExpression();

            var derivedTypeMappingData = declaredTypeMappingData.WithTypes(sourceValue.Type, targetType);

            if (declaredTypeMappingData.IsRoot)
            {
                return GetDerivedTypeRootMapping(derivedTypeMappingData, sourceValue, targetValue);
            }

            if (declaredTypeMapperData.TargetMemberIsEnumerableElement())
            {
                return MappingFactory.GetElementMapping(derivedTypeMappingData, sourceValue, targetValue);
            }

            return GetDerivedTypeChildMapping(derivedTypeMappingData, sourceValue, targetValue);
        }

        private static Expression GetDerivedTypeRootMapping(
            IObjectMappingData derivedTypeMappingData,
            Expression sourceValue,
            Expression targetValue)
        {
            var mappingValues = new MappingValues(
                sourceValue,
                targetValue,
                typeof(int?).ToDefaultExpression());

            // Derived type conversions are performed with ObjectMappingData.As<TDerivedSource, TDerivedTarget>()
            // so no need for createMethod or createMethodCallArguments arguments:
            var inlineMappingBlock = MappingFactory.GetInlineMappingBlock(
                derivedTypeMappingData,
                default(MethodInfo),
                mappingValues,
                Enumerable<Expression>.EmptyArray);

            return inlineMappingBlock;
        }

        private static Expression GetDerivedTypeChildMapping(
            IObjectMappingData derivedTypeMappingData,
            Expression sourceValue,
            Expression targetValue)
        {
            var derivedTypeMapperData = derivedTypeMappingData.MapperData;
            var declaredTypeMapperData = derivedTypeMappingData.DeclaredTypeMappingData.MapperData;

            var mappingValues = new MappingValues(
                sourceValue,
                targetValue,
                derivedTypeMapperData.EnumerableIndex);

            return MappingFactory.GetChildMapping(
                derivedTypeMappingData,
                mappingValues,
                declaredTypeMapperData.DataSourceIndex,
                declaredTypeMapperData);
        }
    }
}