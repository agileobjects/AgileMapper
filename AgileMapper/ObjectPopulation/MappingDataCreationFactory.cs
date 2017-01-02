namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;
    using Extensions;
    using Members;

    internal static class MappingDataCreationFactory
    {
        public static Expression ForDerivedType(ObjectMapperData childMapperData)
        {
            Expression asConversion;

            UseAsConversion(childMapperData, out asConversion);

            return asConversion;
        }

        private static bool UseAsConversion(ObjectMapperData childMapperData, out Expression conversion)
        {
            if (childMapperData.Context.IsStandalone)
            {
                conversion = childMapperData.DeclaredTypeMapperData
                    .GetAsCall(childMapperData.SourceType, childMapperData.TargetType);

                return true;
            }

            conversion = null;
            return false;
        }

        public static Expression ForChild(
            MappingValues mappingValues,
            int dataSourceIndex,
            ObjectMapperData childMapperData)
        {
            Expression asConversion;

            if (UseAsConversion(childMapperData, out asConversion))
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

        public static Expression ForElement(
            MappingValues mappingValues,
            Expression enumerableMappingDataObject,
            ObjectMapperData childMapperData)
        {
            Expression asConversion;

            if (UseAsConversion(childMapperData, out asConversion))
            {
                return asConversion;
            }

            return ForElement(mappingValues, enumerableMappingDataObject);
        }

        public static Expression ForElement(MappingValues mappingValues, ObjectMapperData elementMapperData)
            => ForElement(mappingValues, elementMapperData.Parent.MappingDataObject);

        public static Expression ForElement(MappingValues mappingValues, Expression enumerableMappingDataObject)
        {
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