namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Diagnostics;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;

    internal delegate Expression CreateMappingDataCallFactory(
        MappingValues mappingValues, 
        ObjectMapperData memberMapperData);

    internal static class MappingDataCreationFactory
    {
        [DebuggerStepThrough]
        public static Expression ForToTarget(
            ObjectMapperData parentMapperData,
            Expression toTargetSourceValue)
        {
            var withSourceMethod = parentMapperData
                .MappingDataObject
                .Type
                .GetPublicInstanceMethod("WithSource")
                .MakeGenericMethod(toTargetSourceValue.Type);

            var withSourceCall = Expression.Call(
                parentMapperData.MappingDataObject,
                withSourceMethod,
                toTargetSourceValue);

            return withSourceCall;
        }

        [DebuggerStepThrough]
        public static Expression ForDerivedType(MappingValues mappingValues, ObjectMapperData childMapperData)
        {
            UseAsConversion(childMapperData, out var asConversion);
            return asConversion;
        }

        [DebuggerStepThrough]
        private static bool UseAsConversion(ObjectMapperData childMapperData, out Expression conversion)
        {
            if (childMapperData.Context.IsStandalone)
            {
                conversion = childMapperData
                    .DeclaredTypeMapperData
                    .GetAsCall(childMapperData.SourceType, childMapperData.TargetType);

                return true;
            }

            conversion = null;
            return false;
        }

        [DebuggerStepThrough]
        public static Expression ForChild(MappingValues mappingValues, ObjectMapperData childMapperData)
        {
            if (UseAsConversion(childMapperData, out var asConversion))
            {
                return asConversion;
            }

            var createMethod = MappingDataFactory
                .ForChildMethod
                .MakeGenericMethod(childMapperData.SourceType, childMapperData.TargetType);

            var createCall = Expression.Call(
                createMethod,
                mappingValues.SourceValue,
                mappingValues.TargetValue,
                mappingValues.ElementIndex,
                mappingValues.ElementKey,
                childMapperData.TargetMember.RegistrationName.ToConstantExpression(),
                mappingValues.DataSourceIndex.ToConstantExpression(),
                childMapperData.Parent.MappingDataObject);

            return createCall;
        }

        [DebuggerStepThrough]
        public static Expression ForElement(
            MappingValues mappingValues,
            ObjectMapperData elementMapperData)
        {
            if (UseAsConversion(elementMapperData, out var asConversion))
            {
                return asConversion;
            }

            var createMethod = MappingDataFactory
                .ForElementMethod
                .MakeGenericMethod(mappingValues.SourceValue.Type, mappingValues.TargetValue.Type);

            var enumerableMappingDataObject = elementMapperData.Context.IsStandalone
                ? typeof(IObjectMappingData).ToDefaultExpression()
                : (Expression)elementMapperData.Parent.MappingDataObject;

            var createCall = Expression.Call(
                createMethod,
                mappingValues.SourceValue,
                mappingValues.TargetValue,
                mappingValues.ElementIndex,
                mappingValues.ElementKey.GetConversionToObject(),
                enumerableMappingDataObject);

            return createCall;
        }
    }
}