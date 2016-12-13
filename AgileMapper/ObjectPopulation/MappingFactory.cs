namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;
    using Members;

    internal static class MappingFactory
    {
        #region Derived Type Mappings

        public static Expression GetDerivedTypeMapping(
            IObjectMappingData declaredTypeMappingData,
            Expression sourceValue,
            Type targetType)
        {
            var declaredTypeMapperData = declaredTypeMappingData.MapperData;

            var targetValue = declaredTypeMapperData.TargetMember.IsReadable
                ? declaredTypeMapperData.TargetObject.GetConversionTo(targetType)
                : Expression.Default(targetType);

            var derivedTypeMappingData = declaredTypeMappingData.WithTypes(sourceValue.Type, targetType);

            if (declaredTypeMappingData.IsRoot)
            {
                return GetDerivedTypeRootMapping(derivedTypeMappingData, sourceValue, targetValue);
            }

            if (declaredTypeMapperData.TargetMemberIsEnumerableElement())
            {
                return GetInlineElementMappingBlock(derivedTypeMappingData, sourceValue, targetValue);
            }

            return GetDerivedTypeChildMapping(derivedTypeMappingData, sourceValue, targetValue);
        }

        private static Expression GetDerivedTypeRootMapping(
            IObjectMappingData derivedTypeMappingData,
            Expression sourceValue,
            Expression targetValue)
        {
            var mappingValues = new MappingValues(
                sourceValue,
                targetValue,
                Expression.Default(typeof(int?)));

            // Derived type conversions are performed with ObjectMappingData.As<TDerivedSource, TDerivedTarget>()
            // so no need for createMethod or createMethodCallArguments arguments:
            var inlineMappingBlock = GetInlineMappingBlock(
                derivedTypeMappingData,
                default(MethodInfo),
                mappingValues,
                Enumerable<Expression>.EmptyArray);

            return inlineMappingBlock;
        }

        private static Expression GetDerivedTypeChildMapping(
            IObjectMappingData derivedTypeMappingData,
            Expression sourceValue,
            Expression targetValue)
        {
            var derivedTypeMapperData = derivedTypeMappingData.MapperData;
            var declaredTypeMapperData = derivedTypeMappingData.DeclaredTypeMappingData.MapperData;

            var mappingValues = new MappingValues(
                sourceValue,
                targetValue,
                derivedTypeMapperData.EnumerableIndex);

            return GetChildMapping(
                derivedTypeMappingData,
                mappingValues,
                declaredTypeMapperData.DataSourceIndex,
                declaredTypeMapperData);
        }

        #endregion

        public static Expression GetChildMapping(
            IQualifiedMember sourceMember,
            Expression sourceMemberAccess,
            int dataSourceIndex,
            IChildMemberMappingData childMappingData)
        {
            var childMapperData = childMappingData.MapperData;
            var targetMemberAccess = childMapperData.GetTargetMemberAccess();

            var mappingValues = new MappingValues(
                sourceMemberAccess,
                targetMemberAccess,
                childMapperData.EnumerableIndex);

            var childObjectMappingData = ObjectMappingDataFactory.ForChild(
                sourceMember,
                childMapperData.TargetMember,
                dataSourceIndex,
                childMappingData.Parent);

            if (childObjectMappingData.MapperKey.MappingTypes.RuntimeTypesNeeded)
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

        private static Expression GetChildMapping(
            IObjectMappingData childMappingData,
            MappingValues mappingValues,
            int dataSourceIndex,
            ObjectMapperData declaredTypeMapperData)
        {
            var childMapperData = childMappingData.MapperData;

            if (childMapperData.TargetMemberEverRecurses())
            {
                var mapRecursionCall = GetMapRecursionCallFor(
                    childMappingData,
                    mappingValues.SourceValue,
                    dataSourceIndex,
                    declaredTypeMapperData);

                return mapRecursionCall;
            }

            var inlineMappingBlock = GetInlineMappingBlock(
                childMappingData,
                MappingDataFactory.ForChildMethod,
                mappingValues,
                new[]
                {
                    mappingValues.SourceValue,
                    mappingValues.TargetValue,
                    mappingValues.EnumerableIndex,
                    Expression.Constant(childMapperData.TargetMember.RegistrationName),
                    Expression.Constant(dataSourceIndex),
                    childMapperData.Parent.MappingDataObject
                });

            return inlineMappingBlock;
        }

        private static Expression GetMapRecursionCallFor(
            IObjectMappingData childMappingData,
            Expression sourceValue,
            int dataSourceIndex,
            ObjectMapperData declaredTypeMapperData)
        {
            var childMapperData = childMappingData.MapperData;

            childMapperData.RegisterRequiredMapperFunc(childMappingData);

            var mapRecursionCall = declaredTypeMapperData.GetMapRecursionCall(
                sourceValue,
                childMapperData.TargetMember,
                dataSourceIndex);

            return mapRecursionCall;
        }

        public static Expression GetElementMapping(
            Expression sourceElementValue,
            Expression targetElementValue,
            IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            if (mapperData.TargetMember.IsEnumerable)
            {
                mappingData = ObjectMappingDataFactory.ForElement(mappingData);
            }

            if (mappingData.MapperKey.MappingTypes.RuntimeTypesNeeded)
            {
                return mapperData.GetMapCall(sourceElementValue, targetElementValue);
            }

            return GetInlineElementMappingBlock(mappingData, sourceElementValue, targetElementValue);
        }

        private static Expression GetInlineElementMappingBlock(
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
                parentMappingDataObject = Expression.Default(typeof(IObjectMappingData));
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
                MappingDataFactory.ForElementMethod,
                mappingValues,
                new[]
                {
                    mappingValues.SourceValue,
                    mappingValues.TargetValue,
                    mappingValues.EnumerableIndex,
                    parentMappingDataObject
                });
        }

        private static Expression GetInlineMappingBlock(
            IObjectMappingData childMappingData,
            MethodInfo createMethod,
            MappingValues mappingValues,
            Expression[] createMethodCallArguments)
        {
            var childMapper = childMappingData.Mapper;

            if (childMapper.MappingExpression.NodeType != ExpressionType.Try)
            {
                return childMapper.MappingExpression;
            }

            if (!childMapper.MapperData.Context.UsesMappingDataObject)
            {
                return GetDirectAccessMapping(
                    childMappingData,
                    mappingValues,
                    createMethod,
                    createMethodCallArguments);
            }

            var createInlineMappingDataCall = GetCreateMappingDataCall(
                createMethod,
                childMapper.MapperData,
                createMethodCallArguments);

            var mappingBlock = UseLocalSourceValueVariable(
                childMapper.MapperData.MappingDataObject,
                createInlineMappingDataCall,
                childMapper.MappingExpression);

            return mappingBlock;
        }

        private static Expression GetDirectAccessMapping(
            IObjectMappingData mappingData,
            MappingValues mappingValues,
            MethodInfo createMethod,
            Expression[] createMethodCallArguments)
        {
            var mapperData = mappingData.MapperData;
            var mapping = mappingData.Mapper.MappingLambda.Body;

            var useLocalSourceValueVariable = UseLocalSourceValueVariable(mappingValues.SourceValue, mapping, mapperData);

            Expression sourceValue, sourceValueVariableValue = null;

            if (useLocalSourceValueVariable)
            {
                var sourceValueVariableName = GetSourceValueVariableName(mapperData, mappingValues.SourceValue.Type);
                sourceValue = Expression.Variable(mappingValues.SourceValue.Type, sourceValueVariableName);
                sourceValueVariableValue = mappingValues.SourceValue;
            }
            else
            {
                sourceValue = mappingValues.SourceValue;
            }

            var replacementsByTarget = new ExpressionReplacementDictionary
            {
                [mapperData.SourceObject] = sourceValue,
                [mapperData.TargetObject] = mappingValues.TargetValue,
                [mapperData.EnumerableIndex] = mappingValues.EnumerableIndex.GetConversionTo(mapperData.EnumerableIndex.Type)
            };

            var directAccessMapping = mapping.Replace(replacementsByTarget);

            var createInlineMappingDataCall = GetCreateMappingDataCall(
                createMethod,
                mapperData,
                createMethodCallArguments);

            directAccessMapping = directAccessMapping.Replace(
                mapperData.MappingDataObject,
                createInlineMappingDataCall);

            return useLocalSourceValueVariable
                ? UseLocalSourceValueVariable((ParameterExpression)sourceValue, sourceValueVariableValue, directAccessMapping)
                : directAccessMapping;
        }

        public static bool UseLocalSourceValueVariable(
            Expression sourceValue,
            Expression mapping,
            ObjectMapperData mapperData)
        {
            return (sourceValue.NodeType != ExpressionType.Parameter) &&
                   SourceAccessFinder.MultipleAccessesExist(mapperData, mapping);
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

        private static Expression GetCreateMappingDataCall(
            MethodInfo createMethod,
            ObjectMapperData childMapperData,
            Expression[] createMethodCallArguments)
        {
            if (childMapperData.Context.IsStandalone)
            {
                return childMapperData.DeclaredTypeMapperData
                    .GetAsCall(childMapperData.SourceType, childMapperData.TargetType);
            }

            return Expression.Call(
                createMethod.MakeGenericMethod(childMapperData.SourceType, childMapperData.TargetType),
                createMethodCallArguments);
        }

        public static Expression UseLocalSourceValueVariableIfAppropriate(
            Expression mappingExpression,
            ObjectMapperData mapperData)
        {
            if (mapperData.Context.IsForDerivedType || !mapperData.Context.IsStandalone)
            {
                return mappingExpression;
            }

            if (!UseLocalSourceValueVariable(mapperData.SourceObject, mappingExpression, mapperData))
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
            var variableAssignment = Expression.Assign(variable, variableValue);
            var mappingTryCatch = (TryExpression)body;

            var mappingBody = mappingTryCatch.Body;

            if (performValueReplacement)
            {
                mappingBody = mappingBody.Replace(variableValue, variable);
            }

            var updatedTryCatch = mappingTryCatch.Update(
                Expression.Block(variableAssignment, mappingBody),
                mappingTryCatch.Handlers,
                mappingTryCatch.Finally,
                mappingTryCatch.Fault);

            var mappingBlock = Expression.Block(new[] { variable }, updatedTryCatch);

            return mappingBlock;
        }
    }
}