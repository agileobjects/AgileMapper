namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using Extensions.Internal;
    using Members;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal static class MappingFactory
    {
        public static Expression GetChildMapping(
            IQualifiedMember sourceMember,
            Expression sourceMemberAccess,
            int dataSourceIndex,
            IChildMemberMappingData childMappingData)
        {
            var childMapperData = childMappingData.MapperData;
            var targetMemberAccess = childMapperData.GetTargetMemberAccess();

            childMapperData.TargetMember.MapCreating(sourceMember.Type);

            var mappingValues = new MappingValues(
                sourceMemberAccess,
                targetMemberAccess,
                childMapperData.EnumerableIndex);

            var childObjectMappingData = ObjectMappingDataFactory.ForChild(
                sourceMember,
                childMapperData.TargetMember,
                dataSourceIndex,
                childMappingData.Parent);

            if (childObjectMappingData.MappingTypes.RuntimeTypesNeeded)
            {
                return childMapperData.Parent.GetMapCall(
                    mappingValues.SourceValue,
                    childMapperData.TargetMember,
                    dataSourceIndex);
            }

            return GetChildMapping(
                childObjectMappingData,
                mappingValues,
                dataSourceIndex,
                childMapperData.Parent);
        }

        public static Expression GetChildMapping(
            IObjectMappingData childMappingData,
            MappingValues mappingValues,
            int dataSourceIndex,
            ObjectMapperData declaredTypeMapperData)
        {
            var childMapperData = childMappingData.MapperData;

            if (childMapperData.TargetMemberEverRecurses())
            {
                var mapRecursionCall = childMapperData
                    .RuleSet
                    .RecursiveMemberMappingStrategy
                    .GetMapRecursionCallFor(
                        childMappingData,
                        mappingValues.SourceValue,
                        dataSourceIndex,
                        declaredTypeMapperData);

                return mapRecursionCall;
            }

            var inlineMappingBlock = GetInlineMappingBlock(
                childMappingData,
                mappingValues,
                MappingDataCreationFactory.ForChild(mappingValues, dataSourceIndex, childMapperData));

            return inlineMappingBlock;
        }

        public static Expression GetElementMapping(
            Expression sourceElementValue,
            Expression targetElementValue,
            IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            if (CreateElementMappingDataFor(mapperData))
            {
                mappingData = ObjectMappingDataFactory.ForElement(mappingData);
            }

            mapperData.TargetMember.MapCreating(sourceElementValue.Type);

            if (mappingData.MappingTypes.RuntimeTypesNeeded)
            {
                return mapperData.GetMapCall(sourceElementValue, targetElementValue);
            }

            return GetElementMapping(mappingData, sourceElementValue, targetElementValue);
        }

        private static bool CreateElementMappingDataFor(IBasicMapperData mapperData)
        {
            if (!mapperData.TargetMemberIsEnumerableElement())
            {
                return true;
            }

            if (mapperData.TargetMember.IsEnumerable)
            {
                return !mapperData.TargetMember.ElementType.IsSimple();
            }

            return false;
        }

        public static Expression GetElementMapping(
            IObjectMappingData elementMappingData,
            Expression sourceElementValue,
            Expression targetElementValue)
        {
            var enumerableMapperData = elementMappingData.Parent.MapperData;
            var elementMapperData = elementMappingData.MapperData;

            Expression enumerableIndex, parentMappingDataObject;

            if (elementMapperData.Context.IsStandalone)
            {
                enumerableIndex = Expression.Property(elementMapperData.EnumerableIndex, "Value");
                parentMappingDataObject = typeof(IObjectMappingData).ToDefaultExpression();
            }
            else
            {
                enumerableIndex = enumerableMapperData.EnumerablePopulationBuilder.Counter;
                parentMappingDataObject = enumerableMapperData.MappingDataObject;
            }

            var mappingValues = new MappingValues(
                sourceElementValue,
                targetElementValue,
                enumerableIndex);

            elementMapperData.Context.IsForNewElement = targetElementValue.NodeType == ExpressionType.Default;

            return GetInlineMappingBlock(
                elementMappingData,
                mappingValues,
                MappingDataCreationFactory.ForElement(mappingValues, parentMappingDataObject, elementMapperData));
        }

        public static Expression GetInlineMappingBlock(
            IObjectMappingData mappingData,
            MappingValues mappingValues,
            Expression createMappingDataCall)
        {
            var mapper = mappingData.GetOrCreateMapper();

            if (mapper == null)
            {
                return Constants.EmptyExpression;
            }

            if (mapper.MapperData.Context.UsesMappingDataObject)
            {
                return UseLocalSourceValueVariable(
                    mapper.MapperData.MappingDataObject,
                    createMappingDataCall,
                    mapper.MappingExpression);
            }

            return GetDirectAccessMapping(
                mapper.MappingLambda.Body,
                mapper.MapperData,
                mappingValues,
                createMappingDataCall);
        }

        private static Expression GetDirectAccessMapping(
            Expression mapping,
            IMemberMapperData mapperData,
            MappingValues mappingValues,
            Expression createMappingDataCall)
        {
            var useLocalSourceValueVariable =
                ShouldUseLocalSourceValueVariable(mappingValues.SourceValue, mapping, mapperData);

            Expression sourceValue, sourceValueVariableValue;

            if (useLocalSourceValueVariable)
            {
                var sourceValueVariableName = GetSourceValueVariableName(mapperData, mappingValues.SourceValue.Type);
                sourceValue = Expression.Variable(mappingValues.SourceValue.Type, sourceValueVariableName);
                sourceValueVariableValue = mappingValues.SourceValue;
            }
            else
            {
                sourceValue = mappingValues.SourceValue;
                sourceValueVariableValue = null;
            }

            var replacementsByTarget = new ExpressionReplacementDictionary(3)
            {
                [mapperData.SourceObject] = sourceValue,
                [mapperData.TargetObject] = mappingValues.TargetValue,
                [mapperData.EnumerableIndex] = mappingValues.EnumerableIndex.GetConversionTo(mapperData.EnumerableIndex.Type)
            };

            mapping = mapping
                .Replace(replacementsByTarget)
                .Replace(mapperData.MappingDataObject, createMappingDataCall);

            return useLocalSourceValueVariable
                ? UseLocalSourceValueVariable((ParameterExpression)sourceValue, sourceValueVariableValue, mapping)
                : mapping;
        }

        private static bool ShouldUseLocalSourceValueVariable(
            Expression sourceValue,
            Expression mapping,
            IBasicMapperData mapperData)
        {
            return (sourceValue.NodeType != ExpressionType.Parameter) &&
                   !mapperData.RuleSet.Settings.UseMemberInitialisation &&
                    SourceAccessCounter.MultipleAccessesExist(sourceValue, mapping);
        }

        private static string GetSourceValueVariableName(IMemberMapperData mapperData, Type sourceType = null)
        {
            var sourceValueVariableName = "source" + (sourceType ?? mapperData.SourceType).GetVariableNameInPascalCase();

            var numericSuffix = default(string);

            for (var i = mapperData.MappingDataObject.Name.Length - 1; i > 0; --i)
            {
                if (!char.IsDigit(mapperData.MappingDataObject.Name[i]))
                {
                    break;
                }

                numericSuffix = mapperData.MappingDataObject.Name[i] + numericSuffix;
            }

            return sourceValueVariableName + numericSuffix;
        }

        public static Expression UseLocalSourceValueVariableIfAppropriate(
            Expression mappingExpression,
            ObjectMapperData mapperData)
        {
            if (mapperData.Context.IsForDerivedType ||
               !mapperData.Context.IsStandalone ||
                mapperData.UseSingleMappingExpression())
            {
                return mappingExpression;
            }

            if (!ShouldUseLocalSourceValueVariable(mapperData.SourceObject, mappingExpression, mapperData))
            {
                return mappingExpression;
            }

            var sourceValueVariableName = GetSourceValueVariableName(mapperData);
            var sourceValueVariable = Expression.Variable(mapperData.SourceType, sourceValueVariableName);

            return UseLocalSourceValueVariable(
                sourceValueVariable,
                mapperData.SourceObject,
                mappingExpression,
                performValueReplacement: true);
        }

        private static Expression UseLocalSourceValueVariable(
            ParameterExpression variable,
            Expression variableValue,
            Expression body,
            bool performValueReplacement = false)
        {
            var variableAssignment = variable.AssignTo(variableValue);
            var bodyIsTryCatch = body.NodeType == ExpressionType.Try;
            var tryCatch = bodyIsTryCatch ? (TryExpression)body : null;
            var mappingBody = bodyIsTryCatch ? tryCatch.Body : body;

            if (performValueReplacement)
            {
                mappingBody = mappingBody.Replace(variableValue, variable);
            }

            if (!bodyIsTryCatch)
            {
                return Expression.Block(new[] { variable }, variableAssignment, mappingBody);
            }

            mappingBody = tryCatch.Update(
                Expression.Block(variableAssignment, mappingBody),
                tryCatch.Handlers,
                tryCatch.Finally,
                tryCatch.Fault);

            return Expression.Block(new[] { variable }, mappingBody);
        }
    }
}