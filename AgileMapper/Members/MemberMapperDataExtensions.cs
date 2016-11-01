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

        public static bool TargetMemberReferencesRecursionRoot(this IMemberMapperData mapperData)
        {
            if (!mapperData.TargetMember.IsRecursive)
            {
                return false;
            }

            var parentMapperData = mapperData.Parent;

            while (!parentMapperData.IsRoot)
            {
                if ((parentMapperData.TargetMember.LeafMember == mapperData.TargetMember.LeafMember) &&
                    TargetMemberIsRecursionRoot(parentMapperData))
                {
                    return true;
                }

                parentMapperData = parentMapperData.Parent;
            }

            return false;
        }

        private static bool TargetMemberIsRecursionRoot(IMemberMapperData mapperData)
            => mapperData.TargetMember.IsRecursive && !mapperData.Parent.TargetMember.IsRecursive;

        public static Expression GetTargetMemberAccess(this IMemberMapperData mapperData)
            => mapperData.TargetMember.GetAccess(mapperData.InstanceVariable);

        public static Expression GetMapCall(this IMemberMapperData mapperData, Expression value, int dataSourceIndex = 0)
            => mapperData.Parent.GetMapCall(value, mapperData.TargetMember, dataSourceIndex);

        public static Expression[] GetNestedAccessesIn(this IMemberMapperData mapperData, Expression value)
        {
            return mapperData.NestedAccessFinder.FindIn(
                value,
                mapperData.RuleSet.ComplexTypeMappingShortCircuitStrategy.SourceCanBeNull);
        }

        public static bool IsForInlineMapping(this IMemberMapperData mapperData)
            => !(mapperData.SourceType.RuntimeSourceTypeNeeded() || mapperData.TargetType.RuntimeTargetTypeNeeded());

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
                return mapperData.MappingDataObject;
            }

            // TODO: Use IsForInlineMapping and access parent data objects directly
            Expression dataAccess = mapperData.MappingDataObject;

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

        public static Expression GetTypedContextAccess(this IMemberMapperData mapperData, Expression contextAccess, Type[] contextTypes)
        {
            if (contextAccess == mapperData.MappingDataObject)
            {
                return mapperData.MappingDataObject;
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