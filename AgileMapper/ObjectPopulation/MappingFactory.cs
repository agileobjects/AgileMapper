namespace AgileObjects.AgileMapper.ObjectPopulation
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Caching.Dictionaries;
    using Extensions.Internal;
    using Members;
    using Members.Extensions;
    using NetStandardPolyfills;

    internal static class MappingFactory
    {
        public static Expression GetChildMapping(
            IQualifiedMember sourceMember,
            Expression sourceMemberAccess,
            int dataSourceIndex,
            IChildMemberMappingData childMappingData)
        {
            var childMapperData = childMappingData.MapperData;

            childMapperData.TargetMember.MapCreating(sourceMember.Type);

            var childObjectMappingData = ObjectMappingDataFactory.ForChild(
                sourceMember,
                childMapperData.TargetMember,
                dataSourceIndex,
                childMappingData.Parent);
             
            var targetMemberAccess = childMapperData.GetTargetMemberAccess();

            var mappingValues = new MappingValues(
                sourceMemberAccess,
                targetMemberAccess,
                childMapperData.ElementIndex,
                childMapperData.ElementKey,
                dataSourceIndex);

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
                childMapperData.Parent);
        }

        public static Expression GetChildMapping(
            IObjectMappingData childMappingData,
            MappingValues mappingValues,
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
                        declaredTypeMapperData);

                return repeatMappingCall;
            }

            var inlineMappingBlock = GetInlineMappingBlock(
                childMappingData,
                mappingValues,
                MappingDataCreationFactory.ForChild);

            return inlineMappingBlock;
        }

        public static Expression GetElementMapping(
            Expression sourceElementValue,
            Expression targetElementValue,
            IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            if (CreateElementMappingDataFor(mapperData, sourceElementValue))
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

        private static bool CreateElementMappingDataFor(
            ObjectMapperData mapperData,
            Expression sourceElementValue)
        {
            if (!mapperData.TargetMemberIsEnumerableElement())
            {
                return true;
            }

            if (mapperData.TargetMember.IsEnumerable)
            {
                return !mapperData.EnumerablePopulationBuilder.TargetElementsAreSimple &&
                        sourceElementValue.Type.IsAssignableTo(mapperData.SourceMember.ElementType);
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

            Expression elementIndex, elementKey;

            if (elementMapperData.Context.IsStandalone)
            {
                elementIndex = elementMapperData.ElementIndex.GetNullableValueAccess();
                elementKey = elementMapperData.ElementKey;
            }
            else
            {
                elementIndex = enumerableMapperData.EnumerablePopulationBuilder.Counter;
                elementKey = enumerableMapperData.EnumerablePopulationBuilder.GetElementKey();
            }

            var mappingValues = new MappingValues(
                sourceElementValue,
                targetElementValue,
                elementIndex,
                elementKey,
                enumerableMapperData.DataSourceIndex);

            elementMapperData.Context.IsForNewElement =
                (targetElementValue.NodeType == ExpressionType.Default) ||
                (elementMapperData.DeclaredTypeMapperData?.Context.IsForNewElement == true);

            if (elementMapperData.IsRepeatMapping &&
                elementMapperData.RuleSet.RepeatMappingStrategy.AppliesTo(elementMapperData))
            {
                var repeatMappingCall = elementMapperData
                    .RuleSet
                    .RepeatMappingStrategy
                    .GetMapRepeatedCallFor(
                        elementMappingData,
                        mappingValues,
                        enumerableMapperData);

                return repeatMappingCall;
            }

            var inlineMappingBlock = GetInlineMappingBlock(
                elementMappingData,
                mappingValues,
                MappingDataCreationFactory.ForElement);

            return inlineMappingBlock;
        }

        public static Expression GetInlineMappingBlock(
            IObjectMappingData mappingData,
            MappingValues mappingValues,
            CreateMappingDataCallFactory createMappingDataCallFactory)
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

            var mapperData = mapper.MapperData;

            if (mapperData.Context.UsesMappingDataObject)
            {
                return UseLocalValueVariable(
                    mapperData.MappingDataObject,
                    createMappingDataCallFactory.Invoke(mappingValues, mapperData),
                    mapper.Mapping,
                    mapperData);
            }

            return GetDirectAccessMapping(
                mapper.Mapping,
                mapperData,
                mappingValues,
                createMappingDataCallFactory.Invoke(mappingValues, mapperData));
        }

        public static Expression GetDirectAccessMapping(
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

            //var replacementsByTarget = FixedSizeExpressionReplacementDictionary
            //    .WithEquivalentKeys(4)
            //    .Add(mapperData.SourceObject, sourceValue)
            //    .Add(mapperData.TargetObject, mappingValues.TargetValue)
            //    .Add(mapperData.ElementKey, mappingValues.ElementKey)
            //    .Add(
            //        mapperData.ElementIndex,
            //        mappingValues.ElementIndex.GetConversionTo(mapperData.ElementIndex.Type));

            //mapping = mapping
            //    .Replace(replacementsByTarget)
            //    .Replace(mapperData.MappingDataObject, createMappingDataCall);

            return useLocalSourceValueVariable
                ? UseLocalValueVariable(
                   (ParameterExpression)sourceValue,
                    sourceValueVariableValue,
                    mapping,
                    mapperData)
                : mapping;
        }

        private static bool ShouldUseLocalSourceValueVariable(
            Expression sourceValue,
            Expression mapping,
            IRuleSetOwner ruleSetOwner)
        {
            return (sourceValue.NodeType != ExpressionType.Parameter) &&
                   !ruleSetOwner.RuleSet.Settings.UseMemberInitialisation &&
                    SourceValueUseEvaluator.UseLocalVariableFor(sourceValue, mapping);
        }

        public static Expression UseLocalSourceValueVariableIfAppropriate(
            Expression mappingExpression,
            ObjectMapperData mapperData)
        {
            if (mapperData.IsEntryPoint ||
                mapperData.Context.IsForDerivedType ||
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
                mapperData,
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
                mappingExpression,
                toTargetMapperData);
        }

        private static Expression UseLocalValueVariable(
            ParameterExpression variable,
            Expression variableValue,
            Expression body,
            IMemberMapperData mapperData,
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

            var tryCatch = (body.NodeType != ExpressionType.Try)
                ? body.WrapInTryCatch(mapperData)
                : (TryExpression)body;

            body = tryCatch.Update(
                Expression.Block(variableAssignment, tryCatch.Body.Replace(variableValue, variable)),
                tryCatch.Handlers,
                tryCatch.Finally,
                tryCatch.Fault);

            return Expression.Block(new[] { variable }, body);
        }
    }
}