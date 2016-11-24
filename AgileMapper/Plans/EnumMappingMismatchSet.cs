namespace AgileObjects.AgileMapper.Plans
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using DataSources;
    using Extensions;
    using Members;
    using ReadableExpressions;

    internal class EnumMappingMismatchSet
    {
        private EnumMappingMismatchSet(IEnumerable<EnumMappingMismatch> mappingMismatches)
        {
            var warningsText = string.Join(
                Environment.NewLine + Environment.NewLine,
                mappingMismatches.Where(mm => mm.Any).Select(mm => mm.Warning));

            if (warningsText != string.Empty)
            {
                Warnings = ReadableExpression.Comment(warningsText);
            }
        }

        #region Factory Method

        public static EnumMappingMismatchSet For(
            QualifiedMember targetMember,
            IEnumerable<IDataSource> dataSources,
            IMemberMapperData mapperData)
        {
            var sourceEnumData = dataSources
                .Select(ds => new
                {
                    DataSource = ds,
                    EnumType = ds.SourceMember.Type.GetNonNullableUnderlyingTypeIfAppropriate()
                })
                .GroupBy(dss => dss.EnumType)
                .Select(dsGroup => new
                {
                    EnumType = dsGroup.Key,
                    SourceMembers = dsGroup.Select(ds => ds.DataSource.SourceMember).ToArray()
                })
                .ToArray();

            var targetEnumType = targetMember.Type.GetNonNullableUnderlyingTypeIfAppropriate();
            var targetEnumNames = Enum.GetNames(targetEnumType);

            var mappingMismatches = sourceEnumData
                .Where(d => d.EnumType != targetEnumType)
                .Select(d => EnumMappingMismatch.For(
                    d.EnumType,
                    d.SourceMembers,
                    targetEnumType,
                    targetEnumNames,
                    targetMember,
                    mapperData))
                .ToArray();

            return new EnumMappingMismatchSet(mappingMismatches);
        }

        #endregion

        public bool Any => Warnings != null;

        public Expression Warnings { get; }

        #region EnumMappingMismatch Class

        private class EnumMappingMismatch
        {
            private static readonly EnumMappingMismatch _empty = new EnumMappingMismatch();

            private EnumMappingMismatch()
            {
            }

            private EnumMappingMismatch(
                IQualifiedMember targetMember,
                IEnumerable<IQualifiedMember> sourceMembers,
                string[] mismatches,
                IMemberMapperData mapperData)
            {
                var rootMapperData = mapperData.GetRootMapperData();
                var sourceMemberPaths = string.Join(" / ", sourceMembers.Select(sm => sm.GetFriendlySourcePath(rootMapperData)));
                var targetMemberPath = targetMember.GetFriendlyTargetPath(rootMapperData);

                var warningLines = mismatches
                    .Prepend($"WARNING - enum mismatches mapping {sourceMemberPaths} to {targetMemberPath}:")
                    .ToArray();

                Warning = string.Join(Environment.NewLine, warningLines);
            }

            #region Factory Method

            public static EnumMappingMismatch For(
                Type sourceEnumType,
                IEnumerable<IQualifiedMember> sourceMembers,
                Type targetEnumType,
                string[] targetEnumNames,
                IQualifiedMember targetMember,
                IMemberMapperData mapperData)
            {
                var mismatches = GetMappingMismatches(
                    sourceEnumType,
                    targetEnumType.Name,
                    targetEnumNames);

                if (mismatches.None())
                {
                    return _empty;
                }

                return new EnumMappingMismatch(targetMember, sourceMembers, mismatches, mapperData);
            }

            private static string[] GetMappingMismatches(
                Type sourceEnumType,
                string targetEnumTypeName,
                string[] targetEnumNames)
            {
                var sourceEnumNames = Enum.GetNames(sourceEnumType);
                var unmatchedSourceValues = sourceEnumNames.Except(targetEnumNames).ToArray();
                var unmatchedTargetValues = targetEnumNames.Except(sourceEnumNames).ToArray();

                if (unmatchedSourceValues.None() && unmatchedTargetValues.None())
                {
                    return Constants.EmptyStringArray;
                }

                var unmatchedValues = unmatchedSourceValues
                    .Select(value => $" - {sourceEnumType.Name}.{value} matches no {targetEnumTypeName}")
                    .Concat(unmatchedTargetValues
                        .Select(value => $" - {targetEnumTypeName}.{value} is matched by no {sourceEnumType.Name}"))
                    .ToArray();

                return unmatchedValues;
            }

            #endregion

            public bool Any => Warning != null;

            public string Warning { get; }
        }

        #endregion
    }
}