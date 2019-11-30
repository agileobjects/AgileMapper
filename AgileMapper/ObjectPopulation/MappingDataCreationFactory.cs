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

    internal static class MappingDataCreationFactory
    {
        [DebuggerStepThrough]
        public static Expression ForToTarget(
            ObjectMapperData parentMapperData,
            Expression toTargetDataSource)
        {
            var withSourceMethod = parentMapperData
                .MappingDataObject
                .Type
                .GetPublicInstanceMethod("WithSource")
                .MakeGenericMethod(toTargetDataSource.Type);

            var withSourceCall = Expression.Call(
                parentMapperData.MappingDataObject,
                withSourceMethod,
                toTargetDataSource);

            return withSourceCall;
        }

        [DebuggerStepThrough]
        public static Expression ForDerivedType(ObjectMapperData childMapperData)
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
        public static Expression ForChild(
            MappingValues mappingValues,
            int dataSourceIndex,
            ObjectMapperData childMapperData)
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
                dataSourceIndex.ToConstantExpression(),
                childMapperData.Parent.MappingDataObject);

            return createCall;
        }

        [DebuggerStepThrough]
        public static Expression ForElement(
            MappingValues mappingValues,
            Expression enumerableMappingDataObject,
            ObjectMapperData childMapperData)
        {
            if (UseAsConversion(childMapperData, out var asConversion))
            {
                return asConversion;
            }

            var createMethod = MappingDataFactory
                .ForElementMethod
                .MakeGenericMethod(mappingValues.SourceValue.Type, mappingValues.TargetValue.Type);

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