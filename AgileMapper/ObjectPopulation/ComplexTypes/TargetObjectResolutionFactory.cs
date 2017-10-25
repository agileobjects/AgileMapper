namespace AgileObjects.AgileMapper.ObjectPopulation.ComplexTypes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using Members;

    internal static class TargetObjectResolutionFactory
    {
        public static Expression GetObjectResolution(
            Expression construction,
            IObjectMappingData mappingData,
            bool assignTargetObject = false)
        {
            return GetObjectResolution(
                (md, mps) => construction,
                mappingData,
                null,
                false,
                assignTargetObject);
        }

        public static Expression GetObjectResolution(
            Func<IObjectMappingData, IList<Expression>, Expression> constructionFactory,
            IObjectMappingData mappingData,
            IList<Expression> memberPopulations,
            bool assignCreatedObject = false,
            bool assignTargetObject = false)
        {
            var mapperData = mappingData.MapperData;

            if (mapperData.TargetMember.IsReadOnly || mapperData.TargetIsDefinitelyPopulated())
            {
                return mapperData.TargetObject;
            }

            var objectValue = constructionFactory.Invoke(mappingData, memberPopulations);

            if (objectValue == null)
            {
                mapperData.TargetMember.IsReadOnly = true;

                // Use the existing target object if the mapper can't create an instance:
                return mapperData.TargetObject;
            }

            if (UseNullFallbackValue(mapperData, objectValue, memberPopulations))
            {
                objectValue = mapperData.TargetMember.Type.ToDefaultExpression();
                mapperData.Context.UsesMappingDataObjectAsParameter = false;
            }
            else
            {
                if (assignCreatedObject)
                {
                    mapperData.Context.UsesMappingDataObjectAsParameter = true;
                    objectValue = mapperData.CreatedObject.AssignTo(objectValue);
                }

                if (assignTargetObject || mapperData.Context.UsesMappingDataObjectAsParameter)
                {
                    objectValue = mapperData.TargetObject.AssignTo(objectValue);
                }
            }

            objectValue = AddExistingTargetCheckIfAppropriate(objectValue, mappingData);

            return objectValue;
        }

        private static bool UseNullFallbackValue(
            IMemberMapperData mapperData,
            Expression objectConstruction,
            IList<Expression> memberPopulations)
        {
            if (memberPopulations == null)
            {
                return false;
            }

            if ((objectConstruction.NodeType != ExpressionType.New) ||
                 MemberPopulationsExist(memberPopulations) ||
                 mapperData.SourceMember.Matches(mapperData.TargetMember))
            {
                return false;
            }

            var objectNewing = (NewExpression)objectConstruction;

            return objectNewing.Arguments.None();
        }

        private static bool MemberPopulationsExist(IEnumerable<Expression> populationsAndCallbacks)
            => populationsAndCallbacks.Any(population => population.NodeType != ExpressionType.Constant);

        private static Expression AddExistingTargetCheckIfAppropriate(Expression value, IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            if (mapperData.TargetMemberIsUserStruct() ||
                mapperData.TargetIsDefinitelyUnpopulated())
            {
                return value;
            }

            return Expression.Coalesce(mapperData.TargetObject, value);
        }
    }
}