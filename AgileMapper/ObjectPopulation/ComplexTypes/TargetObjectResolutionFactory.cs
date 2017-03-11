namespace AgileObjects.AgileMapper.ObjectPopulation.ComplexTypes
{
    using System;
    using System.Linq.Expressions;
    using Extensions;
    using Members;

    internal static class TargetObjectResolutionFactory
    {
        public static Expression GetObjectResolution(
            Func<IObjectMappingData, Expression> constructionFactory,
            IObjectMappingData mappingData,
            bool assignCreatedObject = false,
            bool assignTargetObject = false)
        {
            var mapperData = mappingData.MapperData;

            if (mapperData.TargetMember.IsReadOnly || mapperData.TargetIsDefinitelyPopulated())
            {
                return mapperData.TargetObject;
            }

            var objectValue = constructionFactory.Invoke(mappingData);

            if (objectValue == null)
            {
                mapperData.TargetMember.IsReadOnly = true;

                // Use the existing target object if the mapper can't create an instance:
                return mapperData.TargetObject;
            }

            if (assignCreatedObject)
            {
                mapperData.Context.UsesMappingDataObjectAsParameter = true;
                objectValue = mapperData.CreatedObject.AssignTo(objectValue);
            }

            if (assignTargetObject || mapperData.Context.UsesMappingDataObjectAsParameter)
            {
                objectValue = mapperData.TargetObject.AssignTo(objectValue);
            }

            objectValue = AddExistingTargetCheckIfAppropriate(objectValue, mappingData);

            return objectValue;
        }

        private static Expression AddExistingTargetCheckIfAppropriate(Expression value, IObjectMappingData mappingData)
        {
            if (mappingData.MapperData.TargetCouldBePopulated())
            {
                return Expression.Coalesce(mappingData.MapperData.TargetObject, value);
            }

            return value;
        }
    }
}