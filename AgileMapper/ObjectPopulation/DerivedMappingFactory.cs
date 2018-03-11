namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq.Expressions;
    using Extensions.Internal;
    using Members;
    using Members.Dictionaries;

    internal static class DerivedMappingFactory
    {
        public static Expression GetDerivedTypeMapping(
            IObjectMappingData declaredTypeMappingData,
            Expression sourceValue,
            Type targetType)
        {
            var declaredTypeMapperData = declaredTypeMappingData.MapperData;

            var targetValue = UseTargetObject(declaredTypeMapperData)
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

        private static bool UseTargetObject(IBasicMapperData mapperData)
        {
            if (!mapperData.TargetMember.IsReadable)
            {
                return false;
            }

            if (!mapperData.TargetMemberIsEnumerableElement())
            {
                return true;
            }

            if (!(mapperData.TargetMember is DictionaryTargetMember dictionaryTargetMember))
            {
                return true;
            }

            return dictionaryTargetMember.CheckExistingElementValue;
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

            var inlineMappingBlock = MappingFactory.GetInlineMappingBlock(
                derivedTypeMappingData,
                mappingValues,
                MappingDataCreationFactory.ForDerivedType(derivedTypeMappingData.MapperData));

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