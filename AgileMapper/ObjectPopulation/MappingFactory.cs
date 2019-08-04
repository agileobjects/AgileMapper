namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Extensions;
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
                return childMapperData.Parent.GetRuntimeTypedMapping(
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

            if (childMapperData.IsRepeatMapping &&
                childMapperData.RuleSet.RepeatMappingStrategy.AppliesTo(childMapperData))
            {
                var repeatMappingCall = childMapperData
                    .RuleSet
                    .RepeatMappingStrategy
                    .GetMapRepeatedCallFor(
                        childMappingData,
                        mappingValues,
                        dataSourceIndex,
                        declaredTypeMapperData);

                return repeatMappingCall;
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
                return mapperData.GetRuntimeTypedMapping(sourceElementValue, targetElementValue);
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
                enumerableIndex = elementMapperData.EnumerableIndex.GetNullableValueAccess();
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

            if (elementMapperData.IsRepeatMapping &&
                elementMapperData.RuleSet.RepeatMappingStrategy.AppliesTo(elementMapperData))
            {
                var repeatMappingCall = elementMapperData
                    .RuleSet
                    .RepeatMappingStrategy
                    .GetMapRepeatedCallFor(
                        elementMappingData,
                        mappingValues,
                        enumerableMapperData.DataSourceIndex,
                        enumerableMapperData);

                return repeatMappingCall;
            }

            var inlineMappingBlock = GetInlineMappingBlock(
                elementMappingData,
                mappingValues,
                MappingDataCreationFactory.ForElement(mappingValues, parentMappingDataObject, elementMapperData));

            return inlineMappingBlock;
        }

        public static Expression GetInlineMappingBlock(
            IObjectMappingData mappingData,
            MappingValues mappingValues,
            Expression createMappingDataCall)
        {
            var mapper = mappingData.GetOrCreateMapper();

            if (mapper == null)
            {
                if (mappingData.HasSameTypedConfiguredDataSource())
                {
                    // Configured data source for an otherwise-unconstructable complex type:
                    return mappingValues.SourceValue;
                }

                return Constants.EmptyExpression;
            }

            if (mapper.MapperData.Context.UsesMappingDataObject)
            {
                return UseLocalValueVariable(
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
                var sourceValueVariableName = mappingValues.SourceValue.Type.GetSourceValueVariableName();
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
                ? UseLocalValueVariable((ParameterExpression)sourceValue, sourceValueVariableValue, mapping)
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

            var sourceValueVariableName = mapperData.SourceType.GetSourceValueVariableName();
            var sourceValueVariable = Expression.Variable(mapperData.SourceType, sourceValueVariableName);

            return UseLocalValueVariable(
                sourceValueVariable,
                mapperData.SourceObject,
                mappingExpression,
                performValueReplacement: true);
        }

        public static Expression UseLocalToTargetDataSourceVariableIfAppropriate(
            ObjectMapperData mapperData,
            ObjectMapperData toTargetMapperData,
            Expression toTargetDataSourceValue,
            Expression mappingExpression)
        {
            if (!toTargetMapperData.Context.UsesMappingDataObject)
            {
                return mappingExpression;
            }

            return UseLocalValueVariable(
                toTargetMapperData.MappingDataObject,
                MappingDataCreationFactory.ForToTarget(mapperData, toTargetDataSourceValue),
                mappingExpression);
        }

        private static Expression UseLocalValueVariable(
            ParameterExpression variable,
            Expression variableValue,
            Expression body,
            bool performValueReplacement = false)
        {
            var variableAssignment = variable.AssignTo(variableValue);

            if (body.NodeType == ExpressionType.Block)
            {
                if (performValueReplacement)
                {
                    body = body.Replace(variableValue, variable);
                }

                var block = (BlockExpression)body;

                return Expression.Block(
                    block.Variables.Append(variable),
                    block.Expressions.Prepend(variableAssignment));
            }

            var tryCatch = (TryExpression)body;

            body = tryCatch.Update(
                Expression.Block(variableAssignment, tryCatch.Body.Replace(variableValue, variable)),
                tryCatch.Handlers,
                tryCatch.Finally,
                tryCatch.Fault);

            return Expression.Block(new[] { variable }, body);
        }
    }
}