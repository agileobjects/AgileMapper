namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using Configuration;
    using DataSources;
    using Dictionaries;
    using Extensions;
    using Extensions.Internal;
    using NetStandardPolyfills;
    using ObjectPopulation;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using static Member;

    internal static class MemberMapperDataExtensions
    {
        public static bool IsStandalone(this IObjectMappingData mappingData)
            => mappingData.IsRoot || mappingData.MappingTypes.RuntimeTypesNeeded;

        public static bool TargetTypeIsEntity(this IMemberMapperData mapperData)
            => IsEntity(mapperData, mapperData.TargetType, out _);

        public static bool IsEntity(this IMemberMapperData mapperData, Type type, out Member idMember)
        {
            if (type == null)
            {
                idMember = null;
                return false;
            }

            idMember = mapperData
                .MapperContext
                .Naming
                .GetIdentifierOrNull(type);

            return idMember?.IsEntityId() == true;
        }

        public static bool UseSingleMappingExpression(this IBasicMapperData mapperData)
            => mapperData.IsRoot && mapperData.RuleSet.Settings.UseSingleRootMappingExpression;

        public static bool UseMemberInitialisations(this IMemberMapperData mapperData)
            => mapperData.RuleSet.Settings.UseMemberInitialisation || mapperData.Context.IsPartOfUserStructMapping();

        public static bool MapToNullCollections(this IMemberMapperData mapperData)
            => mapperData.MapperContext.UserConfigurations.MapToNullCollections(mapperData);

        [DebuggerStepThrough]
        public static ObjectMapperData GetRootMapperData(this IBasicMapperData mapperData)
        {
            while (!mapperData.IsRoot)
            {
                mapperData = mapperData.Parent;
            }

            return (ObjectMapperData)mapperData;
        }

        public static IBasicMapperData GetElementMapperData(this IMemberMapperData mapperData)
        {
            if (mapperData.TargetMember.IsEnumerable)
            {
                return new BasicMapperData(
                    mapperData.RuleSet,
                    mapperData.SourceType,
                    mapperData.TargetMember.ElementType,
                    mapperData.SourceMember,
                    mapperData.TargetMember.GetElementMember(),
                    mapperData);
            }

            return mapperData;
        }

        public static IEnumerable<ObjectMapperData> EnumerateAllMapperDatas(this ObjectMapperData mapperData)
        {
            yield return mapperData;

            foreach (var childMapperData in mapperData.ChildMapperDatas.SelectMany(md => md.EnumerateAllMapperDatas()))
            {
                yield return childMapperData;
            }
        }

        public static bool TargetCouldBePopulated(this IMemberMapperData mapperData)
            => !TargetIsDefinitelyUnpopulated(mapperData);

        public static bool TargetIsDefinitelyPopulated(this IBasicMapperData mapperData)
        {
            return mapperData.RuleSet.Settings.RootHasPopulatedTarget &&
                  (mapperData.IsRoot || mapperData.TargetMemberIsUserStruct());
        }

        public static bool TargetIsDefinitelyUnpopulated(this IMemberMapperData mapperData)
            => mapperData.Context.IsForNewElement || (mapperData.TargetMember.IsRoot && !mapperData.RuleSet.Settings.RootHasPopulatedTarget);

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

            var subjectMapperData = mapperData.TargetMember.LeafMember.DeclaringType == mapperData.TargetInstance.Type
                ? mapperData
                : mapperData.Parent;

            return mapperData.TargetMember.GetAccess(subjectMapperData.TargetInstance, mapperData);
        }

        [DebuggerStepThrough]
        public static Expression GetTargetMemberDefault(this IBasicMapperData mapperData)
            => mapperData.TargetMember.Type.ToDefaultExpression();

        public static ExpressionInfoFinder.ExpressionInfo GetExpressionInfoFor(
            this IMemberMapperData mapperData,
            Expression value,
            bool targetCanBeNull)
        {
            return mapperData.RuleSet.Settings.GuardAccessTo(value)
                ? mapperData.ExpressionInfoFinder.FindIn(value, targetCanBeNull)
                : ExpressionInfoFinder.EmptyExpressionInfo;
        }

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
            if (mapperData.SourceMember is DictionarySourceMember dictionarySourceMember)
            {
                return dictionarySourceMember;
            }

            // We're mapping a dictionary entry by its runtime type:
            return null;
        }

        public static void RegisterTargetMemberDataSourcesIfRequired(
            this IMemberMapperData mapperData,
            DataSourceSet dataSources)
        {
            mapperData.Parent.DataSourcesByTargetMember.Add(mapperData.TargetMember, dataSources);
        }

        public static bool TargetMemberIsUnmappable(this IMemberMapperData mapperData, out string reason)
        {
            if (!mapperData.RuleSet.Settings.AllowSetMethods &&
                (mapperData.TargetMember.LeafMember.MemberType == MemberType.SetMethod))
            {
                reason = "Set methods are unsupported by rule set '" + mapperData.RuleSet.Name + "'";
                return true;
            }

            if (mapperData.TargetMember.LeafMember.HasMatchingCtorParameter &&
              ((mapperData.Parent?.IsRoot != true) ||
               !mapperData.RuleSet.Settings.RootHasPopulatedTarget))
            {
                reason = "Expected to be populated by constructor parameter";
                return true;
            }

            return TargetMemberIsUnmappable(
                mapperData,
                mapperData.TargetMember,
                md => md.MapperContext.UserConfigurations.QueryDataSourceFactories(md),
                mapperData.MapperContext.UserConfigurations,
                out reason);
        }

        public static bool TargetMemberIsUnmappable<TTMapperData>(
            this TTMapperData mapperData,
            QualifiedMember targetMember,
            Func<TTMapperData, IEnumerable<ConfiguredDataSourceFactory>> configuredDataSourcesFactory,
            UserConfigurationSet userConfigurations,
            out string reason)
            where TTMapperData : IBasicMapperData
        {
            if (targetMember == QualifiedMember.All)
            {
                reason = null;
                return false;
            }

            if (!targetMember.LeafMember.IsEntityId() ||
                 userConfigurations.MapEntityKeys(mapperData) ||
                 configuredDataSourcesFactory.Invoke(mapperData).Any())
            {
                return targetMember.IsUnmappable(out reason);
            }

            // If we're here:
            //   1. TargetMember is an Entity key
            //   2. No configuration exists to allow Entity key Mapping
            //   3. No configured data sources exist

            if (mapperData.RuleSet.Settings.AllowCloneEntityKeyMapping &&
               (mapperData.SourceType == mapperData.TargetType))
            {
                return targetMember.IsUnmappable(out reason);
            }

            reason = "Entity key member";
            return true;
        }

        [DebuggerStepThrough]
        public static bool TargetMemberIsEnumerableElement(this IBasicMapperData mapperData)
            => mapperData.TargetMember.LeafMember.IsEnumerableElement();

        [DebuggerStepThrough]
        public static bool TargetMemberHasInitAccessibleValue(this IMemberMapperData mapperData)
            => mapperData.TargetMember.IsReadable && !mapperData.Context.IsPartOfUserStructMapping();

        [DebuggerStepThrough]
        public static bool TargetMemberIsUserStruct(this IBasicMapperData mapperData)
            => mapperData.TargetMember.IsComplex && mapperData.TargetMember.Type.IsValueType();

        public static bool IsRepeatMapping(this IBasicMapperData mapperData)
        {
            if (mapperData.IsRoot || (mapperData.TargetMember.Depth == 2))
            {
                return false;
            }

            if (mapperData.TargetMember.IsRecursion)
            {
                return true;
            }

            if ((mapperData.TargetMember.Depth == 3) && mapperData.TargetMemberIsEnumerableElement())
            {
                return false;
            }

            if (TargetMemberHasRecursiveObjectGraph(mapperData.TargetMember) == false)
            {
                return false;
            }

            // The target member we're mapping right now isn't recursive, but it has recursion
            // within its child members, and its mapping might be repeated elsewhere within the
            // mapping graph. We therefore check if this member ever repeats; if so we'll map it
            // by calling MapRepeated, and it'll be the entry point of the RepeatedMapperFunc
            // which performs the repeated mapping:
            var rootMember = mapperData.GetRootMapperData().TargetMember;

            return TargetMemberEverRepeatsWithin(rootMember, mapperData.TargetMember);
        }

        private static IEnumerable<Member> GetTargetMembers(Type targetType)
            => GlobalContext.Instance.MemberCache.GetTargetMembers(targetType);

        private static bool TargetMemberHasRecursiveObjectGraph(QualifiedMember targetMember)
        {
            while (true)
            {
                var mappingType = targetMember.IsEnumerable ? targetMember.ElementType : targetMember.Type;

                var nonSimpleChildMembers = GetTargetMembers(mappingType)
                    .Filter(m => !m.IsSimple)
                    .Project(cm => GetNonEnumerableChildMember(targetMember, cm))
                    .ToArray();

                if (nonSimpleChildMembers.None())
                {
                    return false;
                }

                if (nonSimpleChildMembers.Any(cm => cm.IsRecursion))
                {
                    return true;
                }

                foreach (var childMember in nonSimpleChildMembers)
                {
                    if (TargetMemberHasRecursiveObjectGraph(childMember))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private static bool TargetMemberEverRepeatsWithin(QualifiedMember parentMember, IQualifiedMember subjectMember)
        {
            while (true)
            {
                var nonSimpleChildMembers = GetTargetMembers(parentMember.Type)
                    .Filter(m => !m.IsSimple)
                    .ToArray();

                if (nonSimpleChildMembers.None())
                {
                    return false;
                }

                var sameTypedChildMembers = nonSimpleChildMembers
                    .Filter(cm => (cm.IsEnumerable ? cm.ElementType : cm.Type) == subjectMember.Type)
                    .ToArray();

                if (sameTypedChildMembers
                        .Project(cm => GetNonEnumerableChildMember(parentMember, cm))
                        .Any(cm => cm != subjectMember))
                {
                    return true;
                }

                foreach (var childMember in nonSimpleChildMembers)
                {
                    var qualifiedChildMember = GetNonEnumerableChildMember(parentMember, childMember);

                    if (qualifiedChildMember.IsRecursion)
                    {
                        continue;
                    }

                    if (TargetMemberEverRepeatsWithin(qualifiedChildMember, subjectMember))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private static QualifiedMember GetNonEnumerableChildMember(QualifiedMember parentMember, Member childMember)
        {
            var qualifiedChildMember = parentMember.Append(childMember);

            if (qualifiedChildMember.IsEnumerable)
            {
                qualifiedChildMember = qualifiedChildMember.GetElementMember();
            }

            return qualifiedChildMember;
        }

        public static Expression GetFallbackCollectionValue(this IMemberMapperData mapperData)
        {
            var targetMember = mapperData.TargetMember;

            Expression emptyEnumerable;

            if (mapperData.TargetMemberHasInitAccessibleValue())
            {
                var existingValue = mapperData.GetTargetMemberAccess();

                if (mapperData.MapToNullCollections())
                {
                    return existingValue;
                }

                emptyEnumerable = targetMember.Type.GetEmptyInstanceCreation(targetMember.ElementType);

                return Expression.Coalesce(existingValue, emptyEnumerable);
            }

            if (mapperData.MapToNullCollections())
            {
                return targetMember.Type.ToDefaultExpression();
            }

            emptyEnumerable = targetMember.Type.GetEmptyInstanceCreation(targetMember.ElementType);

            return emptyEnumerable.GetConversionTo(targetMember.Type);
        }

        public static bool CanConvert(this IMemberMapperData mapperData, Type sourceType, Type targetType)
            => mapperData.MapperContext.ValueConverters.CanConvert(sourceType, targetType);

        public static Expression GetValueConversion(this IMemberMapperData mapperData, Expression value, Type targetType)
            => mapperData.MapperContext.GetValueConversion(value, targetType);

        public static Expression GetMappingCallbackOrNull(
            this IBasicMapperData basicData,
            CallbackPosition callbackPosition,
            IMemberMapperData mapperData)
        {
            return mapperData
                .MapperContext
                .UserConfigurations
                .GetCallbackOrNull(callbackPosition, basicData, mapperData);
        }


        public static ICollection<Type> GetDerivedSourceTypes(this IMemberMapperData mapperData)
            => GlobalContext.Instance.DerivedTypes.GetTypesDerivedFrom(mapperData.SourceType);

        public static ICollection<Type> GetDerivedTargetTypes(this IMemberMapperData mapperData)
            => GlobalContext.Instance.DerivedTypes.GetTypesDerivedFrom(mapperData.TargetType);

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

            var useParentAccess = false;
            PropertyInfo parentProperty = null;

            while (!mapperData.TypesMatch(contextTypes))
            {
                useParentAccess = useParentAccess || mapperData.IsEntryPoint;

                if (useParentAccess)
                {
                    var dataAccessParentProperty =
                        dataAccess.Type.GetPublicInstanceProperty("Parent") ??
                       (parentProperty ??
                       (parentProperty = typeof(IMappingData).GetPublicInstanceProperty("Parent")));

                    dataAccess = Expression.Property(dataAccess, dataAccessParentProperty);
                }
                else
                {
                    dataAccess = mapperData.Parent.MappingDataObject;
                }

                mapperData = mapperData.Parent;
            }

            return dataAccess;
        }

        public static IMemberMapperData GetAppropriateMappingContext(this IMemberMapperData mapperData, params Type[] contextTypes)
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
            return (mapperData.SourceType.IsAssignableTo(sourceType) || sourceType.IsAssignableTo(mapperData.SourceType)) &&
                   (mapperData.TargetType.IsAssignableTo(targetType) || targetType.IsAssignableTo(mapperData.TargetType));
        }

        public static Expression GetTypedContextAccess(
            this IMemberMapperData mapperData,
            Expression contextAccess,
            Type[] contextTypes)
        {
            if (contextAccess == mapperData.MappingDataObject)
            {
                return GetFinalContextAccess(contextAccess, contextTypes);
            }

            if (contextAccess.Type.IsGenericType())
            {
                var contextAccessTypes = contextAccess.Type.GetGenericTypeArguments();

                if (contextAccessTypes[0].IsAssignableTo(contextTypes[0]) &&
                    contextAccessTypes[1].IsAssignableTo(contextTypes[1]))
                {
                    return GetFinalContextAccess(contextAccess, contextTypes, contextAccessTypes);
                }
            }

            return GetAsCall(contextAccess, contextTypes[0], contextTypes[1]);
        }

        private static Expression GetFinalContextAccess(
            Expression contextAccess,
            IList<Type> contextTypes,
            IList<Type> contextAccessTypes = null)
        {
            if ((contextAccessTypes == null) && !contextAccess.Type.IsGenericType())
            {
                return contextAccess;
            }

            if (contextAccessTypes == null)
            {
                contextAccessTypes = contextAccess.Type.GetGenericTypeArguments();
            }

            if (contextAccessTypes.None(t => t.IsValueType()))
            {
                return contextAccess;
            }

            return GetAsCall(contextAccess, contextTypes[0], contextTypes[1]);
        }

        public static Expression GetTargetMemberPopulation(this IMemberMapperData mapperData, Expression value)
            => mapperData.TargetMember.GetPopulation(value, mapperData);

        public static Expression GetAsCall(this IMemberMapperData mapperData, Type sourceType, Type targetType)
            => GetAsCall(mapperData.MappingDataObject, sourceType, targetType);

        public static Expression GetAsCall(this Expression subject, params Type[] contextTypes)
            => GetAsCall(subject, null, contextTypes);

        public static Expression GetAsCall(this Expression subject, Expression isForDerivedTypeArgument, params Type[] contextTypes)
        {
            if (subject.Type.IsGenericType() &&
                subject.Type.GetGenericTypeArguments().SequenceEqual(contextTypes))
            {
                return subject;
            }

            if (subject.Type == typeof(IMappingData))
            {
                return GetAsCall(subject, typeof(IMappingData).GetPublicInstanceMethod("As"), contextTypes);
            }

            if (isForDerivedTypeArgument == null)
            {
                isForDerivedTypeArgument = true.ToConstantExpression();
            }

            MethodInfo conversionMethod;

            if (contextTypes[0].IsValueType())
            {
                conversionMethod = subject.Type.GetPublicInstanceMethod("WithTargetType");
            }
            else if (contextTypes[1].IsValueType())
            {
                conversionMethod = subject.Type.GetPublicInstanceMethod("WithSourceType");
            }
            else
            {
                conversionMethod = typeof(IObjectMappingDataUntyped).GetPublicInstanceMethod("As");
            }

            return GetAsCall(subject, conversionMethod, contextTypes, isForDerivedTypeArgument);
        }

        private static Expression GetAsCall(
            Expression subject,
            MethodInfo asMethod,
            Type[] typeArguments,
            Expression isForDerivedTypeArgument = null)
        {
            return (isForDerivedTypeArgument != null)
                ? Expression.Call(subject, asMethod.MakeGenericMethod(typeArguments), isForDerivedTypeArgument)
                : Expression.Call(subject, asMethod.MakeGenericMethod(typeArguments));
        }

        public static Expression GetSourceAccess(
            this IMemberMapperData mapperData,
            Expression contextAccess,
            Type sourceType)
        {
            return GetAccess(mapperData, contextAccess, GetSourceAccess, sourceType, mapperData.SourceObject, 0);
        }

        public static Expression GetTargetAccess(
            this IMemberMapperData mapperData,
            Expression contextAccess,
            Type targetType)
        {
            return GetAccess(mapperData, contextAccess, GetTargetAccess, targetType, mapperData.TargetObject, 1);
        }

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

            var contextTypes = contextAccess.Type.GetGenericTypeArguments();

            if (!contextTypes[contextTypesIndex].IsAssignableTo(type))
            {
                return accessMethodFactory.Invoke(contextAccess, type);
            }

            var propertyName = new[] { RootSourceMemberName, RootTargetMemberName }[contextTypesIndex];

            var property = contextAccess.Type.GetPublicInstanceProperty(propertyName) ??
                typeof(IMappingData<,>)
                    .MakeGenericType(contextTypes[0], contextTypes[1])
                    .GetPublicInstanceProperty(propertyName);

            // ReSharper disable once AssignNullToNotNullAttribute
            return Expression.Property(contextAccess, property);
        }

        private static readonly MethodInfo _getSourceMethod = typeof(IMappingData).GetPublicInstanceMethod("GetSource");

        private static Expression GetSourceAccess(Expression subject, Type sourceType)
            => GetAccess(subject, _getSourceMethod, sourceType);

        private static readonly MethodInfo _getTargetMethod = typeof(IMappingData).GetPublicInstanceMethod("GetTarget");

        public static Expression GetTargetAccess(Expression subject, Type targetType)
            => GetAccess(subject, _getTargetMethod, targetType);

        private static Expression GetAccess(Expression subject, MethodInfo method, Type typeArgument)
            => Expression.Call(subject, method.MakeGenericMethod(typeArgument));
    }
}