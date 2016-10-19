namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using Configuration;
    using Extensions;

    internal static class MemberMapperDataExtensions
    {
        public static Expression GetMapCall(this IMemberMapperData mapperData, Expression value, int dataSourceIndex = 0)
            => mapperData.Parent.GetMapCall(value, mapperData.TargetMember, dataSourceIndex);

        public static Expression[] GetNestedAccessesIn(this IMemberMapperData mapperData, Expression value)
        {
            return mapperData.NestedAccessFinder.FindIn(
                value,
                mapperData.RuleSet.ComplexTypeMappingShortCircuitStrategy.SourceCanBeNull);
        }

        private static readonly MethodInfo _asMethod = typeof(IMappingData).GetMethod("As");

        public static Expression GetAppropriateTypedMappingContextAccess(this IMemberMapperData mapperData, Type[] contextTypes)
        {
            var access = mapperData.GetAppropriateMappingContextAccess(contextTypes);
            var typedAccess = mapperData.GetTypedContextAccess(access, contextTypes);

            return typedAccess;
        }

        public static Expression GetAppropriateMappingContextAccess(this IMemberMapperData mapperData, Type[] contextTypes)
        {
            if (mapperData.TypesMatch(contextTypes))
            {
                return mapperData.Parameter;
            }

            Expression dataAccess = mapperData.Parameter;

            if (mapperData.TargetMember.IsSimple)
            {
                mapperData = mapperData.Parent;
            }

            while (!mapperData.TypesMatch(contextTypes))
            {
                dataAccess = Expression.Property(dataAccess, "Parent");
                mapperData = mapperData.Parent;
            }

            return dataAccess;
        }

        public static bool TypesMatch(this IBasicMapperData mapperData, IList<Type> contextTypes)
            => contextTypes[0].IsAssignableFrom(mapperData.SourceType) && contextTypes[1].IsAssignableFrom(mapperData.TargetType);

        public static Expression GetTypedContextAccess(this IMemberMapperData mapperData, Expression contextAccess, Type[] contextTypes)
        {
            if (contextAccess == mapperData.Parameter)
            {
                return mapperData.Parameter;
            }

            return Expression.Call(
                contextAccess,
                _asMethod.MakeGenericMethod(contextTypes[0], contextTypes[1]));
        }

        private static readonly MethodInfo _getSourceMethod = typeof(IMappingData).GetMethod("GetSource");
        private static readonly MethodInfo _getTargetMethod = typeof(IMappingData).GetMethod("GetTarget");

        public static Expression GetSourceAccess(this IMemberMapperData mapperData, Expression contextAccess, Type sourceType)
            => GetAccess(mapperData, contextAccess, GetSourceAccess, sourceType, mapperData.SourceObject);

        public static Expression GetTargetAccess(this IMemberMapperData mapperData, Expression contextAccess, Type targetType)
            => GetAccess(mapperData, contextAccess, GetTargetAccess, targetType, mapperData.TargetObject);

        private static Expression GetAccess(
            IMemberMapperData mapperData,
            Expression contextAccess,
            Func<Expression, Type, Expression> accessMethodFactory,
            Type type,
            Expression directAccessExpression)
        {
            return (contextAccess == mapperData.Parameter)
                ? directAccessExpression
                : accessMethodFactory.Invoke(contextAccess, type);
        }

        public static Expression ReplaceTypedParameterWithUntyped(this IMemberMapperData mapperData, Expression expression)
        {
            var replacementsByTarget = new ExpressionReplacementDictionary
            {
                [mapperData.SourceObject] = GetSourceAccess(Parameters.MappingData, mapperData.SourceType),
                [mapperData.TargetObject] = GetTargetAccess(Parameters.MappingData, mapperData.TargetType)
            };

            return expression.Replace(replacementsByTarget);
        }

        private static Expression GetSourceAccess(Expression dataAccess, Type sourceType)
            => GetAccess(dataAccess, _getSourceMethod, sourceType);

        private static Expression GetTargetAccess(Expression dataAccess, Type targetType)
            => GetAccess(dataAccess, _getTargetMethod, targetType);

        private static Expression GetAccess(Expression subject, MethodInfo method, Type typeArgument)
            => Expression.Call(subject, method.MakeGenericMethod(typeArgument));
    }
}