namespace AgileObjects.AgileMapper.ObjectPopulation.ComplexTypes
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using Members;
    using NetStandardPolyfills;

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
            if (value.Type.IsValueType())
            {
                // A user-defined struct will always be populated, but we can 
                // happily overwrite it because it should never represent an 
                // entity. We may as well use the existing value if it has a
                // parameterless constructor, though:
                if (value.Type.GetPublicInstanceConstructors().ToArray().None())
                {
                    return mappingData.MapperData.TargetObject;
                }

                return value;
            }

            if (mappingData.MapperData.TargetIsDefinitelyUnpopulated())
            {
                return value;
            }

            return Expression.Coalesce(mappingData.MapperData.TargetObject, value);
        }
    }
}