namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Caching.Dictionaries;
    using Extensions;
    using Extensions.Internal;
    using Members;
    using ObjectPopulation;
    using ReadableExpressions.Extensions;

    internal static class TypeConversionExtensions
    {
        public static bool CanConvert(this IMemberMapperData mapperData, Type sourceType, Type targetType)
        {
            return mapperData.MapperContext.ValueConverters.CanConvert(sourceType, targetType) ||
                  (mapperData.HasConfiguredSimpleTypeValueFactories() &&
                   mapperData.QuerySimpleTypeValueFactories(sourceType, targetType).Any());
        }

        public static Expression GetValueConversionOrCreation(
            this IMemberMapperData mapperData,
            Expression value,
            Type targetType)
        {
            if (!mapperData.HasConfiguredSimpleTypeValueFactories())
            {
                return mapperData.GetValueConversion(value, targetType);
            }

            var valueFactories = mapperData
                .QuerySimpleTypeValueFactories(value.Type, targetType)
                .ToArray();

            if (valueFactories.None())
            {
                return mapperData.GetValueConversion(value, targetType);
            }

            return mapperData.GetConversionOrCreationExpression(value, targetType, valueFactories);
        }

        public static Expression GetValueConversion(this IMemberMapperData mapperData, Expression value, Type targetType)
            => mapperData.MapperContext.GetValueConversion(value, targetType);

        private static bool HasConfiguredSimpleTypeValueFactories(this IMemberMapperData mapperData)
            => mapperData.MapperContext.UserConfigurations.HasSimpleTypeValueFactories;

        private static IEnumerable<ConfiguredObjectFactory> QuerySimpleTypeValueFactories(
            this IMemberMapperData mapperData,
            Type sourceType,
            Type targetType)
        {
            if (!targetType.IsSimple())
            {
                return Enumerable<ConfiguredObjectFactory>.Empty;
            }

            var queryMapperData = new BasicMapperData(
                mapperData.RuleSet,
                sourceType,
                targetType.GetNonNullableType(),
                QualifiedMember.All);

            return mapperData
                .MapperContext
                .UserConfigurations
                .QueryObjectFactories(queryMapperData);
        }

        private static Expression GetConversionOrCreationExpression(
            this IMemberMapperData mapperData,
            Expression value,
            Type targetType,
            IList<ConfiguredObjectFactory> valueFactories)
        {
            var simpleMemberMapperData = SimpleMemberMapperData.Create(value, mapperData);
            
            var checkNestedAccesses = 
                simpleMemberMapperData.TargetMemberIsEnumerableElement() &&
                value.Type.CanBeNull();

            var replacements = FixedSizeExpressionReplacementDictionary
                .WithEquivalentKeys(3)
                .Add(simpleMemberMapperData.SourceObject, value)
                .Add(simpleMemberMapperData.TargetObject, mapperData.GetTargetMemberAccess())
                .Add(simpleMemberMapperData.EnumerableIndex, simpleMemberMapperData.EnumerableIndexValue);

            var conversions = valueFactories.ProjectToArray(vf =>
            {
                var factoryExpression = vf.Create(simpleMemberMapperData);
                var condition = vf.GetConditionOrNull(simpleMemberMapperData);

                if (checkNestedAccesses)
                {
                    var nestedAccessChecks = ExpressionInfoFinder.Default
                        .FindIn(factoryExpression, checkMultiInvocations: false)
                        .NestedAccessChecks;

                    if (nestedAccessChecks != null)
                    {
                        condition = condition != null
                            ? Expression.AndAlso(nestedAccessChecks, condition)
                            : nestedAccessChecks;
                    }
                }

                return new SimpleTypeValueFactory
                {
                    Factory = factoryExpression.Replace(replacements).GetConversionTo(targetType),
                    Condition = condition?.Replace(replacements)
                };
            });

            if (conversions.Last().Condition != null)
            {
                conversions = conversions.Append(new SimpleTypeValueFactory
                {
                    Factory = mapperData.GetValueConversion(value, targetType) ??
                              simpleMemberMapperData.GetTargetMemberDefault()
                });
            }
            else if (conversions.Length == 1)
            {
                return conversions[0].Factory;
            }

            return GetConversionExpression(conversions);
        }

        private static Expression GetConversionExpression(IList<SimpleTypeValueFactory> conversions)
        {
            var conversionExpression = default(Expression);
            var conversionIndex = conversions.Count;

            while (true)
            {
                var conversion = conversions[--conversionIndex];

                if (conversionExpression == null)
                {
                    conversionExpression = conversion.Factory;
                    continue;
                }

                conversionExpression = Expression.Condition(
                    conversion.Condition,
                    conversion.Factory,
                    conversionExpression);

                if (conversionIndex == 0)
                {
                    return conversionExpression;
                }
            }
        }

        private class SimpleTypeValueFactory
        {
            public Expression Factory { get; set; }

            public Expression Condition { get; set; }
        }
    }
}
