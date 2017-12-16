﻿namespace AgileObjects.AgileMapper.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using DataSources;
    using Extensions.Internal;
    using Members;
    using ReadableExpressions;

    internal class EnumMappingMismatchSet
    {
        public static readonly IEqualityComparer<EnumMappingMismatchSet> Comparer = new MismatchSetComparer();

        private static readonly EnumMappingMismatchSet _emptySet =
            new EnumMappingMismatchSet(Enumerable<EnumMappingMismatch>.EmptyArray);

        private readonly IList<EnumMappingMismatch> _mappingMismatches;
        private Expression _warning;

        private EnumMappingMismatchSet(IList<EnumMappingMismatch> mappingMismatches)
        {
            _mappingMismatches = mappingMismatches;
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
                .Where(mm => mm.Any)
                .ToArray();

            return mappingMismatches.Any() ? new EnumMappingMismatchSet(mappingMismatches) : _emptySet;
        }

        #endregion

        public bool Any => _mappingMismatches.Any();

        public IEnumerable<EnumMappingMismatch> Mismatches => _mappingMismatches;

        public Expression Warnings => _warning ?? (_warning = CreateWarnings());

        private Expression CreateWarnings()
        {
            var warningsText = string.Join(
                Environment.NewLine + Environment.NewLine,
                _mappingMismatches.Select(mm => mm.Warning));

            return (warningsText != string.Empty)
                ? ReadableExpression.Comment(warningsText)
                : Constants.EmptyExpression;
        }

        #region Helper Classes

        private class MismatchSetComparer : IEqualityComparer<EnumMappingMismatchSet>
        {
            public bool Equals(EnumMappingMismatchSet x, EnumMappingMismatchSet y)
            {
                // ReSharper disable PossibleNullReferenceException
                if (x._mappingMismatches.Count != y._mappingMismatches.Count)
                {
                    return false;
                }
                // ReSharper restore PossibleNullReferenceException

                for (var i = 0; i < x._mappingMismatches.Count; i++)
                {
                    var xMismatch = x._mappingMismatches[i];
                    var yMismatch = y._mappingMismatches[i];

                    if (!xMismatch.Equals(yMismatch))
                    {
                        return false;
                    }
                }

                return true;
            }

            public int GetHashCode(EnumMappingMismatchSet mismatchSet) => 0;
        }

        public class EnumMappingMismatch
        {
            private static readonly EnumMappingMismatch _empty = new EnumMappingMismatch();

            private readonly IMemberMapperData _rootMapperData;
            private readonly IEnumerable<IQualifiedMember> _sourceMembers;
            private readonly IList<string> _mismatches;
            private string _warning;

            private EnumMappingMismatch()
            {
            }

            private EnumMappingMismatch(
                IQualifiedMember targetMember,
                IEnumerable<IQualifiedMember> sourceMembers,
                IList<string> mismatches,
                IMemberMapperData mapperData)
            {
                _rootMapperData = mapperData.GetRootMapperData();
                _sourceMembers = sourceMembers;
                _mismatches = mismatches;
                TargetMemberPath = targetMember.GetFriendlyTargetPath(_rootMapperData);
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
                IEnumerable<string> targetEnumNames,
                IMemberMapperData mapperData)
            {
                var sourceEnumNames = Enum.GetNames(sourceEnumType);
                var unmatchedSourceValues = sourceEnumNames.Except(targetEnumNames).ToList();

                if (unmatchedSourceValues.Any())
                {
                    FilterOutConfiguredPairs(unmatchedSourceValues, sourceEnumType, targetEnumType, mapperData);
                }

                if (unmatchedSourceValues.None())
                {
                    return Enumerable<string>.EmptyArray;
                }

                var mismatches = unmatchedSourceValues
                    .Select(value => $" - {sourceEnumType.Name}.{value} matches no {targetEnumType.Name}")
                    .ToArray();

                return mismatches;
            }

            private static void FilterOutConfiguredPairs(
                ICollection<string> unmatchedSourceValues,
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
                    if (unmatchedSourceValues.Contains(enumParing.FirstEnumMemberName))
                    {
                        unmatchedSourceValues.Remove(enumParing.FirstEnumMemberName);
                    }
                }
            }

            #endregion

            public bool Any => _mismatches?.Any() == true;

            public IEnumerable<string> EnumValues => _mismatches;

            public string TargetMemberPath { get; }

            public string SourceMemberPaths =>
                string.Join(" / ", _sourceMembers.Select(sm => sm.GetFriendlySourcePath(_rootMapperData)));

            public string Warning => _warning ?? (_warning = CreateWarning());

            private string CreateWarning()
            {
                var warningLines = _mismatches
                    .Prepend($"WARNING - enum mismatches mapping {SourceMemberPaths} to {TargetMemberPath}:");

                return string.Join(Environment.NewLine, warningLines);
            }

            public bool Equals(EnumMappingMismatch other)
            {
                if (ReferenceEquals(other, this))
                {
                    return true;
                }

                return false;
            }
        }

        #endregion
    }
}