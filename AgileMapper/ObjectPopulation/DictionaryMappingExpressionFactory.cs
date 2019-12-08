namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using System.Reflection;
    using ComplexTypes;
    using DataSources.Factories;
    using Enumerables.Dictionaries;
    using Extensions;
    using Extensions.Internal;
    using Members;
    using Members.Dictionaries;
    using Members.Population;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;

    internal class DictionaryMappingExpressionFactory : MappingExpressionFactoryBase
    {
        private readonly MemberPopulatorFactory _memberPopulatorFactory;

        public DictionaryMappingExpressionFactory()
        {
            _memberPopulatorFactory = new MemberPopulatorFactory(GetAllTargetMembers);
        }

        #region Target Member Generation

        private static IEnumerable<QualifiedMember> GetAllTargetMembers(ObjectMapperData mapperData)
        {
            var targetMembersFromSource = EnumerateAllTargetMembers(mapperData).ToArray();

            var configuredDataSourceFactories = mapperData.MapperContext
                .UserConfigurations
                .QueryDataSourceFactories<ConfiguredDictionaryEntryDataSourceFactory>()
                .Filter(mapperData, (md, dsf) => dsf.IsFor(md))
                .ToArray();

            if (configuredDataSourceFactories.None())
            {
                return targetMembersFromSource;
            }

            var configuredCustomTargetMembers =
                GetConfiguredTargetMembers(configuredDataSourceFactories, targetMembersFromSource);

            var allTargetMembers = targetMembersFromSource.Append(configuredCustomTargetMembers);

            return allTargetMembers;
        }

        private static IEnumerable<QualifiedMember> EnumerateAllTargetMembers(ObjectMapperData mapperData)
        {
            var sourceMembers = GlobalContext.Instance.MemberCache.GetSourceMembers(mapperData.SourceType);
            var targetDictionaryMember = (DictionaryTargetMember)mapperData.TargetMember;

            var targetMembers = EnumerateTargetMembers(
                sourceMembers,
                targetDictionaryMember,
                mapperData,
                m => m.Name);

            foreach (var targetMember in targetMembers)
            {
                yield return targetMember;
            }

            if (mapperData.IsRoot)
            {
                yield break;
            }

            foreach (var targetMember in GetParentContextFlattenedTargetMembers(mapperData, targetDictionaryMember))
            {
                yield return targetMember;
            }
        }

        private static IEnumerable<DictionaryTargetMember> EnumerateTargetMembers(
            IEnumerable<Member> sourceMembers,
            DictionaryTargetMember targetDictionaryMember,
            ObjectMapperData mapperData,
            Func<Member, string> targetMemberNameFactory)
        {
            foreach (var sourceMember in sourceMembers)
            {
                var targetEntryMemberName = targetMemberNameFactory.Invoke(sourceMember);
                var targetEntryMember = targetDictionaryMember.Append(sourceMember.DeclaringType, targetEntryMemberName);

                if (targetDictionaryMember.HasObjectEntries)
                {
                    targetEntryMember = (DictionaryTargetMember)targetEntryMember.WithType(sourceMember.Type);
                }

                var entryMapperData = new ChildMemberMapperData(targetEntryMember, mapperData);
                var configuredKey = GetCustomKeyOrNull(entryMapperData);

                if (configuredKey != null)
                {
                    targetEntryMember.SetCustomKey(configuredKey);
                }

                if (!sourceMember.IsSimple && !targetDictionaryMember.HasComplexEntries)
                {
                    targetEntryMember = targetEntryMember.WithTypeOf(sourceMember);
                }

                yield return targetEntryMember;
            }
        }

        private static string GetCustomKeyOrNull(IMemberMapperData entryMapperData)
        {
            var dictionaries = entryMapperData.MapperContext.UserConfigurations.Dictionaries;
            var configuredFullKey = dictionaries.GetFullKeyValueOrNull(entryMapperData);

            return configuredFullKey ?? dictionaries.GetMemberKeyOrNull(entryMapperData);
        }

        private static IEnumerable<DictionaryTargetMember> GetParentContextFlattenedTargetMembers(
            ObjectMapperData mapperData,
            DictionaryTargetMember targetDictionaryMember)
        {
            while (mapperData.Parent != null)
            {
                mapperData = mapperData.Parent;

                var sourceMembers = GlobalContext.Instance
                    .MemberCache
                    .GetSourceMembers(mapperData.SourceType)
                    .SelectMany(sm => MatchingFlattenedMembers(sm, targetDictionaryMember))
                    .ToArray();

                var targetMembers = EnumerateTargetMembers(
                    sourceMembers,
                    targetDictionaryMember,
                    mapperData,
                    m => m.Name.StartsWithIgnoreCase(targetDictionaryMember.Name)
                       ? m.Name.Substring(targetDictionaryMember.Name.Length)
                       : m.Name);

                foreach (var targetMember in targetMembers)
                {
                    yield return targetMember;
                }
            }
        }

        private static IEnumerable<Member> MatchingFlattenedMembers(Member sourceMember, IQualifiedMember targetDictionaryMember)
        {
            if (sourceMember.Name.EqualsIgnoreCase(targetDictionaryMember.Name))
            {
                return Enumerable<Member>.Empty;
            }

            if (sourceMember.Name.StartsWithIgnoreCase(targetDictionaryMember.Name))
            {
                // e.g. ValueLine1 -> Value
                return new[] { sourceMember };
            }

            if (!targetDictionaryMember.Name.StartsWithIgnoreCase(sourceMember.Name))
            {
                return Enumerable<Member>.Empty;
            }

            // e.g. Val => Value
            return GetNestedFlattenedMembers(sourceMember, sourceMember.Name, targetDictionaryMember.Name);
        }

        private static IEnumerable<Member> GetNestedFlattenedMembers(
            Member parentMember,
            string sourceMemberNameMatchSoFar,
            string targetMemberName)
        {
            return GlobalContext.Instance
                .MemberCache
                .GetSourceMembers(parentMember.Type)
                .SelectMany(sm =>
                {
                    var flattenedSourceMemberName = sourceMemberNameMatchSoFar + sm.Name;

                    if (!targetMemberName.StartsWithIgnoreCase(flattenedSourceMemberName))
                    {
                        return Enumerable<Member>.Empty;
                    }

                    if (targetMemberName.EqualsIgnoreCase(flattenedSourceMemberName))
                    {
                        return GlobalContext.Instance
                            .MemberCache
                            .GetSourceMembers(sm.Type);
                    }

                    return GetNestedFlattenedMembers(
                        sm,
                        flattenedSourceMemberName,
                        targetMemberName);
                })
                .ToArray();
        }

        private static QualifiedMember[] GetConfiguredTargetMembers(
            IEnumerable<ConfiguredDictionaryEntryDataSourceFactory> configuredDataSourceFactories,
            IList<QualifiedMember> targetMembersFromSource)
        {
            return configuredDataSourceFactories
                .GroupBy(dsf => dsf.TargetDictionaryEntryMember.Name)
                .Project(targetMembersFromSource, (tmfs, group) =>
                {
                    QualifiedMember targetMember = group.First().TargetDictionaryEntryMember;

                    targetMember.IsCustom = tmfs.None(
                        targetMember.Name,
                       (tmn, sourceMember) => sourceMember.RegistrationName == tmn);

                    return targetMember.IsCustom ? targetMember : null;
                })
                .WhereNotNull()
                .ToArray();
        }

        #endregion

        protected override bool TargetCannotBeMapped(IObjectMappingData mappingData, out string reason)
        {
            if (mappingData.MappingTypes.SourceType.IsDictionary())
            {
                return base.TargetCannotBeMapped(mappingData, out reason);
            }

            var targetMember = (DictionaryTargetMember)mappingData.MapperData.TargetMember;

            if ((targetMember.KeyType == typeof(string)) || (targetMember.KeyType == typeof(object)))
            {
                return base.TargetCannotBeMapped(mappingData, out reason);
            }

            reason = "Only string- or object-keyed Dictionaries are supported";
            return true;
        }

        protected override Expression GetNullMappingFallbackValue(IMemberMapperData mapperData)
            => mapperData.GetFallbackCollectionValue();

        protected override IEnumerable<Expression> GetObjectPopulation(MappingCreationContext context)
        {
            Expression population;

            if (!context.MapperData.TargetMember.IsDictionary)
            {
                population = GetDictionaryPopulation(context.MappingData);
                goto ReturnPopulation;
            }

            var assignmentFactory = GetDictionaryAssignmentFactoryOrNull(context, out var useAssignmentOnly);

            if (useAssignmentOnly)
            {
                yield return assignmentFactory.Invoke(context.MappingData);
                yield break;
            }

            population = GetDictionaryPopulation(context.MappingData);
            var assignment = assignmentFactory?.Invoke(context.MappingData);

            if (assignment != null)
            {
                yield return assignment;
            }

        ReturnPopulation:
            if (population != null)
            {
                yield return population;
            }
        }

        private static Func<IObjectMappingData, Expression> GetDictionaryAssignmentFactoryOrNull(
            MappingCreationContext context,
            out bool useAssignmentOnly)
        {
            if (!context.InstantiateLocalVariable || context.MapperData.TargetMember.IsReadOnly)
            {
                useAssignmentOnly = false;
                return null;
            }

            var mapperData = context.MapperData;

            if (SourceMemberIsDictionary(mapperData, out var sourceDictionaryMember))
            {
                if (UseDictionaryCloneConstructor(sourceDictionaryMember, mapperData, out var cloneConstructor))
                {
                    useAssignmentOnly = true;
                    return md => GetClonedDictionaryAssignment(md.MapperData, cloneConstructor);
                }

                useAssignmentOnly = false;
                return md => GetMappedDictionaryAssignment(sourceDictionaryMember, md);
            }

            useAssignmentOnly = false;
            return GetParameterlessDictionaryAssignment;
        }

        private static bool SourceMemberIsDictionary(
            IMemberMapperData mapperData,
            out DictionarySourceMember sourceDictionaryMember)
        {
            sourceDictionaryMember = mapperData.GetDictionarySourceMemberOrNull();
            return sourceDictionaryMember != null;
        }

        private static bool UseDictionaryCloneConstructor(
            IQualifiedMember sourceDictionaryMember,
            IQualifiedMemberContext context,
            out ConstructorInfo cloneConstructor)
        {
            cloneConstructor = null;

            return (sourceDictionaryMember.Type == context.TargetType) &&
                    context.TargetMember.ElementType.IsSimple() &&
                  ((cloneConstructor = GetDictionaryCloneConstructor(context)) != null);
        }

        private static ConstructorInfo GetDictionaryCloneConstructor(ITypePair mapperData)
        {
            var dictionaryTypes = mapperData.TargetType.GetDictionaryTypes();
            var dictionaryInterfaceType = typeof(IDictionary<,>).MakeGenericType(dictionaryTypes.Key, dictionaryTypes.Value);

            var comparerProperty = mapperData.SourceType.GetPublicInstanceProperty("Comparer");

            return FindDictionaryConstructor(
                mapperData.TargetType,
                dictionaryInterfaceType,
               (comparerProperty != null) ? 2 : 1);
        }

        private static Expression GetClonedDictionaryAssignment(IMemberMapperData mapperData, ConstructorInfo cloneConstructor)
        {
            Expression cloneDictionary;

            if (cloneConstructor.GetParameters().Length == 1)
            {
                cloneDictionary = Expression.New(cloneConstructor, mapperData.SourceObject);
            }
            else
            {
                var comparer = Expression.Property(mapperData.SourceObject, "Comparer");
                cloneDictionary = Expression.New(cloneConstructor, mapperData.SourceObject, comparer);
            }

            var assignment = mapperData.TargetInstance.AssignTo(cloneDictionary);

            return assignment;
        }

        private static ConstructorInfo FindDictionaryConstructor(
            Type dictionaryType,
            Type firstParameterType,
            int numberOfParameters)
        {
            if (dictionaryType.IsInterface())
            {
                dictionaryType = GetConcreteDictionaryType(dictionaryType);
            }

            return dictionaryType
                .GetPublicInstanceConstructors()
                .Project(ctor => new { Ctor = ctor, Parameters = ctor.GetParameters() })
                .First(ctor =>
                    (ctor.Parameters.Length == numberOfParameters) &&
                    (ctor.Parameters[0].ParameterType == firstParameterType))
                .Ctor;
        }

        private static Type GetConcreteDictionaryType(Type dictionaryInterfaceType)
        {
            var types = dictionaryInterfaceType.GetDictionaryTypes();

            return typeof(Dictionary<,>).MakeGenericType(types.Key, types.Value);
        }

        private static Expression GetMappedDictionaryAssignment(
            DictionarySourceMember sourceDictionaryMember,
            IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            if (UseParameterlessConstructor(sourceDictionaryMember, mapperData))
            {
                return GetParameterlessDictionaryAssignment(mappingData);
            }

            var comparerProperty = mapperData.SourceObject.Type.GetPublicInstanceProperty("Comparer");

            var comparer = Expression.Property(mapperData.SourceObject, comparerProperty);

            var constructor = FindDictionaryConstructor(
                mapperData.TargetType,
                comparer.Type,
                numberOfParameters: 1);

            var dictionaryConstruction = Expression.New(constructor, comparer);

            return GetDictionaryAssignment(dictionaryConstruction, mappingData);
        }

        private static bool UseParameterlessConstructor(
            DictionarySourceMember sourceDictionaryMember,
            IQualifiedMemberContext context)
        {
            if (sourceDictionaryMember.Type.IsInterface())
            {
                return true;
            }

            return sourceDictionaryMember.ValueType != context.TargetMember.ElementType;
        }

        private static Expression GetParameterlessDictionaryAssignment(IObjectMappingData mappingData)
        {
            var helper = mappingData.MapperData.EnumerablePopulationBuilder.TargetTypeHelper;
            var newDictionary = helper.GetEmptyInstanceCreation();

            return GetDictionaryAssignment(newDictionary, mappingData);
        }

        private static Expression GetDictionaryAssignment(Expression value, IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            if (mapperData.TargetMember.IsReadOnly)
            {
                return null;
            }

            var valueResolution = TargetObjectResolutionFactory.GetObjectResolution(
                value,
                mappingData,
                mapperData.HasRepeatedMapperFuncs);

            if (valueResolution == mapperData.TargetInstance)
            {
                return null;
            }

            return mapperData.TargetInstance.AssignTo(valueResolution);
        }

        private Expression GetDictionaryPopulation(IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            if (mapperData.SourceMember.IsEnumerable)
            {
                return GetEnumerableToDictionaryMapping(mappingData);
            }

            var memberPopulations = _memberPopulatorFactory
                .Create(mappingData)
                .Project(memberPopulation => memberPopulation.GetPopulation())
                .ToArray();

            if (memberPopulations.None())
            {
                return null;
            }

            if (memberPopulations.HasOne())
            {
                return memberPopulations[0];
            }

            var memberPopulationBlock = Expression.Block(memberPopulations);

            return memberPopulationBlock;
        }

        private static Expression GetEnumerableToDictionaryMapping(IObjectMappingData mappingData)
        {
            var builder = new DictionaryPopulationBuilder(mappingData.MapperData.EnumerablePopulationBuilder);

            if (builder.HasSourceEnumerable)
            {
                builder.AssignSourceVariableFromSourceObject();
            }

            builder.AddItems(mappingData);

            return builder;
        }
    }
}