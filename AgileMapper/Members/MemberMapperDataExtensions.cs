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
        public static Expression GetMapCall(this MemberMapperData data, Expression value, int dataSourceIndex = 0)
            => data.Parent.GetMapCall(value, data.TargetMember, dataSourceIndex);

        public static Expression[] GetNestedAccessesIn(this MemberMapperData data, Expression value)
        {
            return data.NestedAccessFinder.FindIn(
                value,
                data.RuleSet.ComplexTypeMappingShortCircuitStrategy.SourceCanBeNull);
        }

        private static readonly MethodInfo _asMethod = typeof(IMappingData).GetMethod("As", Constants.PublicInstance);

        public static Expression GetAppropriateTypedMappingContextAccess(this MemberMapperData data, Type[] contextTypes)
        {
            var access = data.GetAppropriateMappingContextAccess(contextTypes);
            var typedAccess = data.GetTypedContextAccess(access, contextTypes);

            return typedAccess;
        }

        public static Expression GetAppropriateMappingContextAccess(this MemberMapperData data, Type[] contextTypes)
        {
            if (data.TypesMatch(contextTypes))
            {
                return data.Parameter;
            }

            Expression dataAccess = data.Parameter;

            if (data.TargetMember.IsSimple)
            {
                data = data.Parent;
            }

            while (!data.TypesMatch(contextTypes))
            {
                dataAccess = Expression.Property(dataAccess, "Parent");
                data = data.Parent;
            }

            return dataAccess;
        }

        public static bool TypesMatch(this BasicMapperData data, IList<Type> contextTypes)
            => contextTypes[0].IsAssignableFrom(data.SourceType) && contextTypes[1].IsAssignableFrom(data.TargetType);

        public static Expression GetTypedContextAccess(this MemberMapperData data, Expression contextAccess, Type[] contextTypes)
        {
            if (contextAccess == data.Parameter)
            {
                return data.Parameter;
            }

            return Expression.Call(
                contextAccess,
                _asMethod.MakeGenericMethod(contextTypes[0], contextTypes[1]));
        }

        private static readonly MethodInfo _getSourceMethod = typeof(IMappingData).GetMethod("GetSource", Constants.PublicInstance);
        private static readonly MethodInfo _getTargetMethod = typeof(IMappingData).GetMethod("GetTarget", Constants.PublicInstance);

        public static Expression GetSourceAccess(this MemberMapperData data, Expression contextAccess, Type sourceType)
            => GetAccess(data, contextAccess, GetSourceAccess, sourceType, data.SourceObject);

        public static Expression GetTargetAccess(this MemberMapperData data, Expression contextAccess, Type targetType)
            => GetAccess(data, contextAccess, GetTargetAccess, targetType, data.TargetObject);

        private static Expression GetAccess(
            MemberMapperData data,
            Expression contextAccess,
            Func<Expression, Type, Expression> accessMethodFactory,
            Type type,
            Expression directAccessExpression)
        {
            return (contextAccess == data.Parameter)
                ? directAccessExpression
                : accessMethodFactory.Invoke(contextAccess, type);
        }

        public static Expression ReplaceTypedParameterWithUntyped(this MemberMapperData data, Expression expression)
        {
            var replacementsByTarget = new Dictionary<Expression, Expression>(EquivalentMemberAccessComparer.Instance)
            {
                [data.SourceObject] = GetSourceAccess(Parameters.MappingData, data.SourceType),
                [data.TargetObject] = GetTargetAccess(Parameters.MappingData, data.TargetType)
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