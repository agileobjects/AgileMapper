﻿namespace AgileObjects.AgileMapper.Validation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Configuration;
    using DataSources;
    using Extensions;
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;
    using ObjectPopulation;
    using ReadableExpressions.Extensions;

    internal static class MappingValidator
    {
        public static void Validate(Mapper mapper)
        {
            var rootMapperDatas = mapper.Context.ObjectMapperFactory.RootMappers.Project(m => m.MapperData);

            VerifyMappingPlanIsComplete(GetAllMapperDatas(rootMapperDatas));
        }

        public static void Validate(ObjectMapperData mapperData)
        {
            VerifyMappingPlanIsComplete(GetAllMapperDatas(new[] { mapperData }));
        }

        public static void Validate(MappingConfigInfo configInfo)
        {
            var creationCallbackKey = new MapperCreationCallbackKey(
                configInfo.RuleSet,
                configInfo.SourceType,
                configInfo.TargetType);

            configInfo.MapperContext.ObjectMapperFactory.RegisterCreationCallback(
                creationCallbackKey,
                createdMapper => VerifyMappingPlanIsComplete(new[] { createdMapper.MapperData }));
        }

        private static void VerifyMappingPlanIsComplete(IEnumerable<ObjectMapperData> mapperDatas)
        {
            var incompleteMappingData = GetIncompleteMappingPlanData(mapperDatas);

            if (incompleteMappingData.None())
            {
                return;
            }

            var previousRootMapperData = default(IMemberMapperData);

            var failureMessage = new StringBuilder();

            foreach (var mappingData in incompleteMappingData)
            {
                if (failureMessage.Length != 0)
                {
                    failureMessage.AppendLine().AppendLine();
                }

                var rootData = mappingData.RootMapperData;

                AddMappingTypeHeaderIfRequired(failureMessage, rootData, ref previousRootMapperData);

                failureMessage
                    .Append(" Rule set: ").AppendLine(rootData.RuleSet.Name).AppendLine();

                AddUnconstructableTargetTypesInfo(mappingData.UnconstructableTargetTypes, failureMessage);
                AddUnmappedTargetMembersInfo(mappingData.UnmappedMembers, failureMessage, rootData);
                AddUnpairedEnumsInfo(mappingData.UnpairedEnums, failureMessage);
            }

            throw new MappingValidationException(failureMessage.ToString());
        }

        private static ICollection<IncompleteMappingData> GetIncompleteMappingPlanData(
            IEnumerable<ObjectMapperData> mapperDatas)
        {
            return mapperDatas
                .Project(md => new
                {
                    MapperData = md,
                    IsUnconstructable = TargetIsUnconstructable(md),
                    UnmappedMembers = md
                        .DataSourcesByTargetMember
                        .Filter(pair => !pair.Value.HasValue)
                        .ToArray(),
                    UnpairedEnums = EnumMappingMismatchFinder.FindMismatches(md)
                })
                .Filter(d => d.IsUnconstructable || d.UnmappedMembers.Any() || d.UnpairedEnums.Any())
                .GroupBy(d => d.MapperData.GetRootMapperData())
                .Project(g => new IncompleteMappingData
                {
                    RootMapperData = g.Key,
                    UnconstructableTargetTypes = g
                        .Filter(d => d.IsUnconstructable)
                        .Project(d => d.MapperData)
                        .ToArray(),
                    UnmappedMembers = g
                        .SelectMany(d => d.UnmappedMembers)
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                    UnpairedEnums = g
                        .SelectMany(d => d.UnpairedEnums)
                        .ToArray()
                })
                .ToArray();
        }

        private static bool TargetIsUnconstructable(ObjectMapperData mapperData)
        {
            if (!mapperData.TargetMember.IsComplex ||
                 mapperData.TargetIsDefinitelyPopulated())
            {
                return false;
            }

            if (mapperData.TargetType.GetPublicInstanceConstructor(Constants.EmptyTypeArray) != null)
            {
                return false;
            }

            if (mapperData.DataSourcesByTargetMember.Any(ds => ds.Key.IsConstructorParameter() && ds.Value.HasValue))
            {
                return false;
            }

            var configuredFactories = mapperData
                .MapperContext
                .UserConfigurations
                .GetObjectFactories(mapperData);

            return configuredFactories.None();
        }

        private static void AddMappingTypeHeaderIfRequired(
            StringBuilder failureMessage,
            IMemberMapperData rootData,
            ref IMemberMapperData previousRootMapperData)
        {
            if ((previousRootMapperData != null) &&
                (previousRootMapperData.SourceType == rootData.SourceType) &&
                (previousRootMapperData.TargetType == rootData.TargetType))
            {
                return;
            }

            var sourcePath = rootData.SourceMember.GetFriendlySourcePath(rootData);
            var targetPath = rootData.TargetMember.GetFriendlyTargetPath(rootData);

            failureMessage
                .AppendLine("- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -")
                .AppendLine("- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -")
                .Append("- ").Append(sourcePath).Append(" -> ").AppendLine(targetPath)
                .AppendLine("- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -")
                .AppendLine("- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -")
                .AppendLine();

            previousRootMapperData = rootData;
        }

        private static void AddUnconstructableTargetTypesInfo(
            ICollection<ObjectMapperData> unmappableTargetTypeData,
            StringBuilder failureMessage)
        {
            if (unmappableTargetTypeData.None())
            {
                return;
            }

            failureMessage
                .AppendLine(" Unconstructable target Types - fix by ignoring or configuring constructor parameters:")
                .AppendLine();

            foreach (var unmappableTypeData in unmappableTargetTypeData)
            {
                var sourceTypeName = unmappableTypeData.SourceType.GetFriendlyName();
                var targetTypeName = unmappableTypeData.TargetType.GetFriendlyName();

                failureMessage.Append("  - ").Append(sourceTypeName).Append(" -> ").AppendLine(targetTypeName);
            }

            failureMessage.AppendLine();
        }

        private static void AddUnmappedTargetMembersInfo(
            Dictionary<QualifiedMember, IDataSourceSet> unmappedMembers,
            StringBuilder failureMessage,
            IMemberMapperData rootData)
        {
            if (unmappedMembers.None())
            {
                return;
            }

            failureMessage
                .AppendLine(" Unmapped target members - fix by ignoring or configuring a custom source member or data source:")
                .AppendLine();

            foreach (var unmappedMember in unmappedMembers)
            {
                var targetMemberPath = unmappedMember.Key.GetFriendlyTargetPath(rootData);

                failureMessage.Append("  - ").AppendLine(targetMemberPath);
            }

            failureMessage.AppendLine();
        }

        private static void AddUnpairedEnumsInfo(
            ICollection<EnumMappingMismatchSet> unpairedEnums,
            StringBuilder failureMessage)
        {
            if (unpairedEnums.None())
            {
                return;
            }

            failureMessage
                .AppendLine(" Unpaired enum values - fix by configuring enum pairs:");

            foreach (var mismatch in unpairedEnums.SelectMany(e => e.Mismatches))
            {
                failureMessage
                    .AppendLine()
                    .Append("  - ").Append(mismatch.SourceMemberPaths).Append(" -> ").Append(mismatch.TargetMemberPath)
                    .AppendLine(":");

                foreach (var valuePair in mismatch.EnumValues)
                {
                    failureMessage.Append("   ").Append(valuePair).AppendLine();
                }
            }
        }

        private static IEnumerable<ObjectMapperData> GetAllMapperDatas(IEnumerable<ObjectMapperData> mapperDatas)
            => mapperDatas.SelectMany(md => md.EnumerateAllMapperDatas());

        #region Helper Class

        private class IncompleteMappingData
        {
            public IMemberMapperData RootMapperData { get; set; }

            public ICollection<ObjectMapperData> UnconstructableTargetTypes { get; set; }

            public Dictionary<QualifiedMember, IDataSourceSet> UnmappedMembers { get; set; }

            public ICollection<EnumMappingMismatchSet> UnpairedEnums { get; set; }
        }

        #endregion
    }
}
