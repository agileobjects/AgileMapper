namespace AgileObjects.AgileMapper.ObjectPopulation.ComplexTypes
{
    using System;
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;

    internal static class TargetObjectResolutionFactory
    {
        public static Expression GetObjectResolution(
            Expression construction,
            IObjectMappingData mappingData,
            bool assignTargetObject = false)
        {
            return GetObjectResolution(
                (_, _) => construction,
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
                if (MarkUnconstructableMemberAsReadOnly(mappingData))
                {
                    mapperData.TargetMember.IsReadOnly = true;
                }

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

            objectValue = AddExistingTargetCheckIfAppropriate(objectValue, mapperData);
            return objectValue;
        }

        private static bool MarkUnconstructableMemberAsReadOnly(IObjectMappingData mappingData)
        {
            if (mappingData.HasSameTypedConfiguredDataSource())
            {
                // Configured data source for an otherwise-unconstructable complex type:
                return false;
            }
                
            // Don't set an non-readonly abstract member to readonly as we'll try to map
            // it from derived types:
            return !mappingData.MapperData.TargetMember.Type.IsAbstract();
        }

        private static bool UseNullFallbackValue(
            IQualifiedMemberContext context,
            Expression objectConstruction,
            IList<Expression> memberPopulations)
        {
            if (memberPopulations == null)
            {
                return false;
            }

            if ((objectConstruction.NodeType != ExpressionType.New) ||
                 MemberPopulationsExist(memberPopulations) ||
                 context.SourceMember.Matches(context.TargetMember))
            {
                return false;
            }

            var objectNewing = (NewExpression)objectConstruction;

            return objectNewing.Arguments.None();
        }

        private static bool MemberPopulationsExist(IList<Expression> populationsAndCallbacks)
            => populationsAndCallbacks.Any(population => population.NodeType != ExpressionType.Constant);

        private static Expression AddExistingTargetCheckIfAppropriate(Expression value, IMemberMapperData mapperData)
        {
            if ((value.NodeType == ExpressionType.Default) ||
                 mapperData.RuleSet.Settings.UseSingleRootMappingExpression ||
                !mapperData.TargetMember.IsReadable ||
                 mapperData.TargetMemberIsUserStruct() ||
                 mapperData.TargetIsDefinitelyUnpopulated())
            {
                return value;
            }

            return Expression.Coalesce(mapperData.TargetObject, value);
        }
    }
}