namespace AgileObjects.AgileMapper.ObjectPopulation.ComplexTypes
{
    using System;
    using System.Collections.Generic;
    using Extensions.Internal;
    using Members;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

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

                // Use the existing target object if it might have a value and
                // the mapper can't create an instance:
                return mapperData.TargetCouldBePopulated()
                    ? mapperData.TargetObject
                    : mapperData.GetTargetMemberDefault();
            }

            if (UseNullFallbackValue(mapperData, objectValue, memberPopulations))
            {
                objectValue = mapperData.GetTargetMemberDefault();
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

        private static bool MemberPopulationsExist(IList<Expression> populationsAndCallbacks)
            => populationsAndCallbacks.Any(population => population.NodeType != ExpressionType.Constant);

        private static Expression AddExistingTargetCheckIfAppropriate(Expression value, IObjectMappingData mappingData)
        {
            if ((value.NodeType == ExpressionType.Default) ||
                 mappingData.MapperData.RuleSet.Settings.UseSingleRootMappingExpression ||
                 mappingData.MapperData.TargetMemberIsUserStruct() ||
                 mappingData.MapperData.TargetIsDefinitelyUnpopulated())
            {
                return value;
            }

            return Expression.Coalesce(mappingData.MapperData.TargetObject, value);
        }
    }
}