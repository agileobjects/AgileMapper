namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions.Internal;
    using Members;

    internal static class DerivedMappingFactory
    {
        public static Expression GetDerivedTypeMapping(
            IObjectMappingData declaredTypeMappingData,
            Expression sourceValue,
            Type targetType)
        {
            return GetDerivedTypeMapping(
                declaredTypeMappingData,
                sourceValue,
                targetType,
                out _);
        }

        public static Expression GetDerivedTypeMapping(
            IObjectMappingData declaredTypeMappingData,
            Expression sourceValue,
            Type targetType,
            out IObjectMappingData derivedTypeMappingData)
        {
            derivedTypeMappingData = declaredTypeMappingData.WithTypes(sourceValue.Type, targetType);

            var declaredTypeMapperData = declaredTypeMappingData.MapperData;

            if (DerivedSourceTypeIsUnconditionallyIgnored(derivedTypeMappingData))
            {
                return declaredTypeMapperData.TargetObject.GetConversionTo(targetType);
            }

            var targetValue = declaredTypeMapperData.TargetMember.IsReadable
                ? declaredTypeMapperData.TargetObject.GetConversionTo(targetType)
                : targetType.ToDefaultExpression();

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

        private static bool DerivedSourceTypeIsUnconditionallyIgnored(IObjectMappingData derivedTypeMappingData)
        {
            var derivedTypeMapperData = derivedTypeMappingData.MapperData;
            var userConfigurations = derivedTypeMapperData.MapperContext.UserConfigurations;

            if (!userConfigurations.HasSourceMemberIgnores)
            {
                return false;
            }

            var derivedTypeSourceMemberIgnore = userConfigurations
                .GetSourceMemberIgnoreOrNull(derivedTypeMapperData);

            return derivedTypeSourceMemberIgnore?.HasConfiguredCondition == false;
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
                derivedTypeMapperData.ElementIndex);

            return MappingFactory.GetChildMapping(
                derivedTypeMappingData,
                mappingValues,
                declaredTypeMapperData.DataSourceIndex,
                declaredTypeMapperData);
        }
    }
}