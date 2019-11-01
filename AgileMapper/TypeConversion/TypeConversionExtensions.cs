namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Extensions;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
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

            var sourceType = value.Type.GetNonNullableType();

            var valueFactories = mapperData
                .QuerySimpleTypeValueFactories(sourceType, targetType)
                .ToArray();

            if (valueFactories.None())
            {
                return mapperData.GetValueConversion(value, targetType);
            }

            var simpleMemberMapperData = SimpleMemberMapperData.Create(value, mapperData);

            var replacements = new ExpressionReplacementDictionary(3)
            {
                [simpleMemberMapperData.SourceObject] = value,
                [simpleMemberMapperData.TargetObject] = mapperData.GetTargetMemberAccess(),
                [simpleMemberMapperData.EnumerableIndex] = simpleMemberMapperData.EnumerableIndexValue
            };

            var conversions = valueFactories
                .ProjectToArray(vf => new
                {
                    Value = vf.Create(simpleMemberMapperData).Replace(replacements).GetConversionTo(targetType),
                    Condition = vf.GetConditionOrNull(simpleMemberMapperData)?.Replace(replacements)
                });

            if (valueFactories.Last().HasConfiguredCondition)
            {
                conversions = conversions.Append(new
                {
                    Value = mapperData.GetValueConversion(value, targetType) ?? mapperData.GetTargetMemberDefault(),
                    Condition = default(Expression)
                });
            }

            var conversionCount = conversions.Length;
            var conversionExpression = default(Expression);

            for (var i = conversionCount; i > 0;)
            {
                var conversion = conversions[--i];

                if (conversionExpression == null)
                {
                    conversionExpression = conversion.Value;
                    continue;
                }

                conversionExpression = Expression.Condition(
                    conversion.Condition,
                    conversion.Value,
                    conversionExpression);
            }

            return conversionExpression;
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
    }
}
