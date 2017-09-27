﻿namespace AgileObjects.AgileMapper.Plans
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
                    EnumType = ds.SourceMember.Type.GetNonNullableType()
                })
                .GroupBy(dss => dss.EnumType)
                .Select(dsGroup => new
                {
                    EnumType = dsGroup.Key,
                    SourceMembers = dsGroup.Select(ds => ds.DataSource.SourceMember).ToArray()
                })
                .ToArray();

            var targetEnumType = targetMember.Type.GetNonNullableType();
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
                    .Prepend($"WARNING - enum mismatches mapping {sourceMemberPaths} to {targetMemberPath}:");

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
                    targetEnumType,
                    targetEnumNames,
                    mapperData);

                if (mismatches.None())
                {
                    return _empty;
                }

                return new EnumMappingMismatch(targetMember, sourceMembers, mismatches, mapperData);
            }

            private static string[] GetMappingMismatches(
                Type sourceEnumType,
                Type targetEnumType,
                string[] targetEnumNames,
                IMemberMapperData mapperData)
            {
                var sourceEnumNames = Enum.GetNames(sourceEnumType);
                var unmatchedSourceValues = sourceEnumNames.Except(targetEnumNames).ToList();
                var unmatchedTargetValues = targetEnumNames.Except(sourceEnumNames).ToList();

                Filter(unmatchedSourceValues, unmatchedTargetValues, sourceEnumType, targetEnumType, mapperData);

                if (unmatchedSourceValues.None() && unmatchedTargetValues.None())
                {
                    return Enumerable<string>.EmptyArray;
                }

                var unmatchedValues = unmatchedSourceValues
                    .Select(value => $" - {sourceEnumType.Name}.{value} matches no {targetEnumType.Name}")
                    .Concat(unmatchedTargetValues
                        .Select(value => $" - {targetEnumType.Name}.{value} is matched by no {sourceEnumType.Name}"))
                    .ToArray();

                return unmatchedValues;
            }

            private static void Filter(
                ICollection<string> unmatchedSourceValues,
                ICollection<string> unmatchedTargetValues,
                Type sourceEnumType,
                Type targetEnumType,
                IMemberMapperData mapperData)
            {
                var relevantEnumPairings = mapperData
                    .MapperContext
                    .UserConfigurations
                    .GetEnumPairingsFor(sourceEnumType, targetEnumType);

                foreach (var enumParing in relevantEnumPairings)
                {
                    if (unmatchedSourceValues.Contains(enumParing.FirstEnumMemberName) &&
                        unmatchedTargetValues.Contains(enumParing.SecondEnumMemberName))
                    {
                        unmatchedSourceValues.Remove(enumParing.FirstEnumMemberName);
                        unmatchedTargetValues.Remove(enumParing.SecondEnumMemberName);
                    }
                }
            }

            #endregion

            public bool Any => Warning != null;

            public string Warning { get; }
        }

        #endregion
    }
}