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

        public static bool TargetCouldBePopulated(this IMemberMapperData mapperData)
            => !TargetIsDefinitelyUnpopulated(mapperData);

        public static bool TargetIsDefinitelyPopulated(this IMemberMapperData mapperData)
            => mapperData.IsRoot && mapperData.RuleSet.RootHasPopulatedTarget;

        public static bool TargetIsDefinitelyUnpopulated(this IMemberMapperData mapperData)
            => mapperData.Context.IsForNewElement || (mapperData.IsRoot && !mapperData.RuleSet.RootHasPopulatedTarget);

        public static bool HasSameSourceAsParent(this IMemberMapperData mapperData)
        {
            if (mapperData.Context.IsStandalone)
            {
                return false;
            }

            return mapperData.SourceMember.Matches(mapperData.Parent.SourceMember);
        }

        public static Expression GetTargetMemberAccess(this IMemberMapperData mapperData)
        {
            if (mapperData.Context.IsStandalone)
            {
                return mapperData.TargetObject;
            }

            var subjectMapperData = mapperData.TargetMember.LeafMember.DeclaringType == mapperData.InstanceVariable.Type
                ? mapperData
                : mapperData.Parent;

            return mapperData.TargetMember.GetAccess(subjectMapperData.InstanceVariable, mapperData);
        }

        public static Expression[] GetNestedAccessesIn(this IMemberMapperData mapperData, Expression value, bool targetCanBeNull)
            => mapperData.NestedAccessFinder.FindIn(value, targetCanBeNull);

        public static bool SourceMemberIsStringKeyedDictionary(
            this IMemberMapperData mapperData,
            out DictionarySourceMember dictionarySourceMember)
        {
            dictionarySourceMember = mapperData.GetDictionarySourceMemberOrNull();

            if (dictionarySourceMember == null)
            {
                return false;
            }

            return dictionarySourceMember.KeyType == typeof(string);
        }

        public static DictionarySourceMember GetDictionarySourceMemberOrNull(this IMemberMapperData mapperData)
        {
            var dictionarySourceMember = mapperData.SourceMember as DictionarySourceMember;

            if (dictionarySourceMember != null)
            {
                return dictionarySourceMember;
            }

            var dictionaryEntrySourceMember = mapperData.SourceMember as DictionaryEntrySourceMember;

            if (dictionaryEntrySourceMember == null)
            {
                return null;
            }

            if (dictionaryEntrySourceMember.Type.IsDictionary())
            {
                return dictionaryEntrySourceMember.Parent;
            }

            // We're mapping a dictionary entry by its runtime type:
            return null;
        }

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
                return targetMember.Type.ToDefaultExpression();
            }

            emptyEnumerable = targetMember.Type.GetEmptyInstanceCreation(targetMember.ElementType);

            return emptyEnumerable.GetConversionTo(targetMember.Type);
        }

        public static Expression GetValueConversion(this IMemberMapperData mapperData, Expression value, Type targetType)
            => mapperData.MapperContext.ValueConverters.GetConversion(value, targetType);

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
                dataAccess = mapperData.Context.IsStandalone || mapperData.TargetMember.IsRecursionRoot()
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

        public static Expression GetTargetMemberPopulation(this IMemberMapperData mapperData, Expression value)
        {
            if (value.Type == typeof(void))
            {
                return value;
            }

            return mapperData.TargetMember.GetPopulation(value, mapperData);
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

        private static Expression GetSourceAccess(Expression subject, Type sourceType)
            => GetAccess(subject, _getSourceMethod, sourceType);

        private static readonly MethodInfo _getTargetMethod = typeof(IMappingData).GetMethod("GetTarget");

        private static Expression GetTargetAccess(Expression subject, Type targetType)
            => GetAccess(subject, _getTargetMethod, targetType);

        private static Expression GetAccess(Expression subject, MethodInfo method, Type typeArgument)
            => Expression.Call(subject, method.MakeGenericMethod(typeArgument));
    }
}