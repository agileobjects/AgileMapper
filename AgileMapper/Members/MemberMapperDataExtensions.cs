namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;
    using NetStandardPolyfills;
    using ObjectPopulation;

    internal static class MemberMapperDataExtensions
    {
        public static IMemberMapperData GetRootMapperData(this IMemberMapperData mapperData)
        {
            while (!mapperData.IsRoot)
            {
                mapperData = mapperData.Parent;
            }

            return mapperData;
        }

        public static bool HasSameSourceAsParent(this IMemberMapperData mapperData)
        {
            if (mapperData.IsRoot)
            {
                return false;
            }

            if (mapperData.SourceMember.Matches(mapperData.Parent.SourceMember))
            {
                return true;
            }

            return false;
        }

        public static Expression GetTargetMemberAccess(this IMemberMapperData mapperData)
        {
            return mapperData.Context.IsStandalone
                ? mapperData.TargetObject
                : mapperData.TargetMember.GetAccess(mapperData.InstanceVariable);
        }

        public static Expression[] GetNestedAccessesIn(this IMemberMapperData mapperData, Expression value, bool targetCanBeNull)
            => mapperData.NestedAccessFinder.FindIn(value, targetCanBeNull);

        public static bool TargetMemberIsEnumerableElement(this IBasicMapperData mapperData)
            => mapperData.TargetMember.LeafMember.IsEnumerableElement();

        public static bool TargetMemberEverRecurses(this IMemberMapperData mapperData)
        {
            if (mapperData.TargetMember.IsRecursive)
            {
                return true;
            }

            var parentMapperData = mapperData.Parent;

            while (!parentMapperData.Context.IsStandalone)
            {
                if (parentMapperData.TargetMember.IsRecursive)
                {
                    // The target member we're mapping right now isn't recursive,
                    // but it's being mapped as part of the mapping of a recursive
                    // member. We therefore check if this member recurses later;
                    // if so we'll map it by calling MapRecursion:
                    return TargetMemberRecursesWithin(
                        parentMapperData.TargetMember,
                        mapperData.TargetMember.LeafMember);
                }

                parentMapperData = parentMapperData.Parent;
            }

            return false;
        }

        private static bool TargetMemberRecursesWithin(QualifiedMember parentMember, Member member)
        {
            var nonSimpleChildMembers = GlobalContext.Instance
                .MemberFinder
                .GetTargetMembers(parentMember.Type)
                .Where(m => !m.IsSimple)
                .ToArray();

            if (nonSimpleChildMembers.Contains(member))
            {
                var childMember = parentMember.Append(member);

                return childMember.IsRecursive;
            }

            return nonSimpleChildMembers.Any(m => TargetMemberRecursesWithin(parentMember.Append(m), member));
        }

        public static bool HasUseableSourceDictionary(this IMemberMapperData mapperData)
        {
            if (!mapperData.SourceType.IsDictionary())
            {
                return false;
            }

            var keyAndValueTypes = mapperData.SourceType.GetGenericArguments();

            if (keyAndValueTypes[0] != typeof(string))
            {
                return false;
            }

            var valueType = keyAndValueTypes[1];
            Type targetType;

            if (mapperData.TargetMember.IsEnumerable)
            {
                targetType = mapperData.TargetMember.ElementType;

                if ((valueType == typeof(object)) || (valueType == targetType) ||
                    targetType.IsComplex() || valueType.IsEnumerable())
                {
                    return true;
                }
            }
            else
            {
                targetType = mapperData.TargetMember.Type;
            }

            return mapperData.MapperContext.ValueConverters.CanConvert(valueType, targetType);
        }

        public static Expression GetFallbackCollectionValue(this IMemberMapperData mapperData)
        {
            var targetMember = mapperData.TargetMember;
            var mapToNullCollections = mapperData.MapperContext.UserConfigurations.MapToNullCollections(mapperData);

            Expression emptyEnumerable;

            if (targetMember.IsReadable)
            {
                var existingValue = mapperData.GetTargetMemberAccess();

                if (mapToNullCollections)
                {
                    return existingValue;
                }

                emptyEnumerable = targetMember.Type.GetEmptyInstanceCreation(targetMember.ElementType);

                return Expression.Coalesce(existingValue, emptyEnumerable);
            }

            if (mapToNullCollections)
            {
                return Expression.Default(targetMember.Type);
            }

            emptyEnumerable = targetMember.Type.GetEmptyInstanceCreation(targetMember.ElementType);

            return emptyEnumerable.GetConversionTo(targetMember.Type);
        }

        public static Expression GetAppropriateTypedMappingContextAccess(this IMemberMapperData mapperData, Type[] contextTypes)
        {
            var access = mapperData.GetAppropriateMappingContextAccess(contextTypes);
            var typedAccess = mapperData.GetTypedContextAccess(access, contextTypes);

            return typedAccess;
        }

        public static Expression GetAppropriateMappingContextAccess(this IMemberMapperData mapperData, params Type[] contextTypes)
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
                dataAccess = mapperData.Context.IsStandalone
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

        private static bool TypesMatch(IBasicMapperData mapperData, Type sourceType, Type targetType)
        {
            return (sourceType.IsAssignableFrom(mapperData.SourceType) || mapperData.SourceType.IsAssignableFrom(sourceType)) &&
                (targetType.IsAssignableFrom(mapperData.TargetType) || mapperData.TargetType.IsAssignableFrom(targetType));
        }

        public static Expression GetTypedContextAccess(
            this IMemberMapperData mapperData,
            Expression contextAccess,
            Type[] contextTypes)
        {
            if (contextAccess == mapperData.MappingDataObject)
            {
                return mapperData.MappingDataObject;
            }

            if (contextAccess.Type.IsGenericType())
            {
                var contextAccessTypes = contextAccess.Type.GetGenericArguments();

                if (contextTypes[0].IsAssignableFrom(contextAccessTypes[0]) &&
                    contextTypes[1].IsAssignableFrom(contextAccessTypes[1]))
                {
                    return contextAccess;
                }
            }

            return GetAsCall(contextAccess, contextTypes[0], contextTypes[1]);
        }

        public static Expression GetAsCall(this IMemberMapperData mapperData, Type sourceType, Type targetType)
            => GetAsCall(mapperData.MappingDataObject, sourceType, targetType);

        private static readonly MethodInfo _mappingDataAsMethod = typeof(IMappingData).GetMethod("As");
        private static readonly MethodInfo _objectMappingDataAsMethod = typeof(IObjectMappingDataUntyped).GetMethod("As");

        public static Expression GetAsCall(this Expression subject, params Type[] contextTypes)
        {
            var method = (subject.Type == typeof(IMappingData))
                ? _mappingDataAsMethod
                : _objectMappingDataAsMethod;

            return Expression.Call(subject, method.MakeGenericMethod(contextTypes));
        }

        public static Expression GetSourceAccess(this IMemberMapperData mapperData, Expression contextAccess, Type sourceType)
            => GetAccess(mapperData, contextAccess, GetSourceAccess, sourceType, mapperData.SourceObject, 0);

        public static Expression GetTargetAccess(this IMemberMapperData mapperData, Expression contextAccess, Type targetType)
            => GetAccess(mapperData, contextAccess, GetTargetAccess, targetType, mapperData.TargetObject, 1);

        private static Expression GetAccess(
            IMemberMapperData mapperData,
            Expression contextAccess,
            Func<Expression, Type, Expression> accessMethodFactory,
            Type type,
            Expression directAccessExpression,
            int contextTypesIndex)
        {
            if (contextAccess == mapperData.MappingDataObject)
            {
                return directAccessExpression;
            }

            if (!contextAccess.Type.IsGenericType())
            {
                return accessMethodFactory.Invoke(contextAccess, type);
            }

            var contextTypes = contextAccess.Type.GetGenericArguments();

            if (!type.IsAssignableFrom(contextTypes[contextTypesIndex]))
            {
                return accessMethodFactory.Invoke(contextAccess, type);
            }

            var propertyName = new[] { "Source", "Target" }[contextTypesIndex];

            var property = contextAccess.Type.GetProperty(propertyName)
                ?? typeof(IMappingData<,>)
                    .MakeGenericType(contextTypes[0], contextTypes[1])
                    .GetProperty(propertyName);

            return Expression.Property(contextAccess, property);
        }

        private static readonly MethodInfo _getSourceMethod = typeof(IMappingData).GetMethod("GetSource");

        private static Expression GetSourceAccess(Expression dataAccess, Type sourceType)
            => GetAccess(dataAccess, _getSourceMethod, sourceType);

        private static readonly MethodInfo _getTargetMethod = typeof(IMappingData).GetMethod("GetTarget");

        private static Expression GetTargetAccess(Expression dataAccess, Type targetType)
            => GetAccess(dataAccess, _getTargetMethod, targetType);

        private static Expression GetAccess(Expression subject, MethodInfo method, Type typeArgument)
            => Expression.Call(subject, method.MakeGenericMethod(typeArgument));
    }
}