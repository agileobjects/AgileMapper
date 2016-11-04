namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;

    internal static class MemberMapperDataExtensions
    {
        public static bool HasSameSourceAsParent(this IMemberMapperData mapperData)
            => !mapperData.IsRoot && mapperData.SourceMember.Matches(mapperData.Parent.SourceMember);

        public static Expression GetTargetMemberAccess(this IMemberMapperData mapperData)
            => mapperData.TargetMember.GetAccess(mapperData.InstanceVariable);

        public static Expression[] GetNestedAccessesIn(this IMemberMapperData mapperData, Expression value)
            => mapperData.NestedAccessFinder.FindIn(value);

        public static bool TargetMemberIsEnumerableElement(this IMemberMapperData mapperData)
            => mapperData.TargetMember.IsEnumerableElement();

        public static bool IsForStandaloneMapping(this IMemberMapperData mapperData)
            => mapperData.SourceType.RuntimeTypeNeeded() || mapperData.TargetType.RuntimeTypeNeeded();

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
                return mapperData.MappingDataObject;
            }

            Expression dataAccess = mapperData.MappingDataObject;

            if (mapperData.TargetMember.IsSimple)
            {
                mapperData = mapperData.Parent;
            }

            while (!mapperData.TypesMatch(contextTypes))
            {
                dataAccess = mapperData.IsForStandaloneMapping
                    ? Expression.Property(dataAccess, "Parent")
                    : (Expression)mapperData.Parent.MappingDataObject;

                mapperData = mapperData.Parent;
            }

            return dataAccess;
        }

        public static IMemberMapperData GetAppropriateMappingContext(this IMemberMapperData mapperData, Type[] contextTypes)
        {
            if (mapperData.TypesMatch(contextTypes))
            {
                return mapperData;
            }

            if (mapperData.TargetMember.IsSimple)
            {
                mapperData = mapperData.Parent;
            }

            while (!mapperData.TypesMatch(contextTypes))
            {
                mapperData = mapperData.Parent;
            }

            return mapperData;
        }

        public static bool TypesMatch(this IBasicMapperData mapperData, IList<Type> contextTypes)
            => TypesMatch(mapperData, contextTypes[0], contextTypes[1]);

        public static bool TypesMatch(this IBasicMapperData mapperData, Type sourceType, Type targetType)
        {
            return sourceType.IsAssignableFrom(mapperData.SourceType) &&
                (targetType.IsAssignableFrom(mapperData.TargetType) || mapperData.TargetType.IsAssignableFrom(targetType));
        }

        private static readonly MethodInfo _asMethod = typeof(IMappingData).GetMethod("As");

        public static Expression GetTypedContextAccess(this IMemberMapperData mapperData, Expression contextAccess, Type[] contextTypes)
        {
            if (contextAccess == mapperData.MappingDataObject)
            {
                return mapperData.MappingDataObject;
            }

            var contextAccessTypes = contextAccess.Type.GetGenericArguments();

            if (contextTypes[0].IsAssignableFrom(contextAccessTypes[0]) &&
                contextTypes[1].IsAssignableFrom(contextAccessTypes[1]))
            {
                return contextAccess;
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
            return (contextAccess == mapperData.MappingDataObject)
                ? directAccessExpression
                : accessMethodFactory.Invoke(contextAccess, type);
        }

        private static Expression GetSourceAccess(Expression dataAccess, Type sourceType)
            => GetAccess(dataAccess, _getSourceMethod, sourceType);

        private static Expression GetTargetAccess(Expression dataAccess, Type targetType)
            => GetAccess(dataAccess, _getTargetMethod, targetType);

        private static Expression GetAccess(Expression subject, MethodInfo method, Type typeArgument)
            => Expression.Call(subject, method.MakeGenericMethod(typeArgument));
    }
}