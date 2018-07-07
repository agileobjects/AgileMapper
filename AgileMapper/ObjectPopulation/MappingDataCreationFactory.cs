namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Diagnostics;
    using Extensions.Internal;
    using Members;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal static class MappingDataCreationFactory
    {
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

        //[DebuggerStepThrough]
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

            var targetMemberRegistrationName = childMapperData.TargetMember.RegistrationName.ToConstantExpression();
            var dataSourceIndexConstant = dataSourceIndex.ToConstantExpression();

            var createCall = Expression.Call(
                createMethod,
                mappingValues.SourceValue,
                mappingValues.TargetValue,
                mappingValues.EnumerableIndex,
                targetMemberRegistrationName,
                dataSourceIndexConstant,
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
                mappingValues.EnumerableIndex,
                enumerableMappingDataObject);

            return createCall;
        }
    }
}