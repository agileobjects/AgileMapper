﻿namespace AgileObjects.AgileMapper.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using DataSources;
    using Extensions;
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;
    using ReadableExpressions;

    internal class EnumMappingMismatchSet
    {
        public static readonly IEqualityComparer<EnumMappingMismatchSet> Comparer = default(MismatchSetComparer);

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
                .Project(ds => new
                {
                    DataSource = ds,
                    EnumType = ds.SourceMember.Type.GetNonNullableType()
                })
                .GroupBy(dss => dss.EnumType)
                .Project(dsGroup => new
                {
                    EnumType = dsGroup.Key,
                    SourceMembers = dsGroup.Project(ds => ds.DataSource.SourceMember).ToArray()
                })
                .ToArray();

            var targetEnumType = targetMember.Type.GetNonNullableType();
            var targetEnumNames = Enum.GetNames(targetEnumType);

            var mappingMismatches = sourceEnumData
                .Filter(targetEnumType, (tet, d) => d.EnumType != tet)
                .Project(d => EnumMappingMismatch.For(
                    d.EnumType,
                    d.SourceMembers,
                    targetEnumType,
                    targetEnumNames,
                    targetMember,
                    mapperData))
                .Filter(mm => mm.Any)
                .ToArray();

            return mappingMismatches.Any() ? new EnumMappingMismatchSet(mappingMismatches) : _emptySet;
        }

        #endregion

        public bool Any => _mappingMismatches.Any();

        public IEnumerable<EnumMappingMismatch> Mismatches => _mappingMismatches;

        public Expression Warnings => _warning ?? (_warning = CreateWarnings());

        private Expression CreateWarnings()
        {
            var warningsText = _mappingMismatches
                .Project(mm => mm.Warning)
                .Join(Environment.NewLine + Environment.NewLine);

            return (warningsText != string.Empty)
                ? ReadableExpression.Comment(warningsText)
                : Constants.EmptyExpression;
        }

        #region Helper Classes

        private struct MismatchSetComparer : IEqualityComparer<EnumMappingMismatchSet>
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
                IQualifiedMemberContext context)
            {
                _rootMapperData = context.GetRootMapperData();
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
                IMapperContextOwner mapperContextOwner)
            {
                var sourceEnumNames = Enum.GetNames(sourceEnumType);
                var unmatchedSourceValues = sourceEnumNames.Except(targetEnumNames).ToList();

                if (unmatchedSourceValues.Any())
                {
                    FilterOutConfiguredPairs(unmatchedSourceValues, sourceEnumType, targetEnumType, mapperContextOwner);
                }

                if (unmatchedSourceValues.None())
                {
                    return Enumerable<string>.EmptyArray;
                }

                var mismatches = unmatchedSourceValues
                    .ProjectToArray(value => $" - {sourceEnumType.Name}.{value} matches no {targetEnumType.Name}");

                return mismatches;
            }

            private static void FilterOutConfiguredPairs(
                ICollection<string> unmatchedSourceValues,
                Type sourceEnumType,
                Type targetEnumType,
                IMapperContextOwner mapperContextOwner)
            {
                var relevantEnumPairings = mapperContextOwner
                    .MapperContext
                    .UserConfigurations
                    .GetEnumPairingsFor(sourceEnumType, targetEnumType);

                foreach (var enumParing in relevantEnumPairings)
                {
                    if (unmatchedSourceValues.Contains(enumParing.PairingEnumMemberName))
                    {
                        unmatchedSourceValues.Remove(enumParing.PairingEnumMemberName);
                    }
                }
            }

            #endregion

            public bool Any => _mismatches?.Any() == true;

            public IEnumerable<string> EnumValues => _mismatches;

            public string TargetMemberPath { get; }

            public string SourceMemberPaths =>
                _sourceMembers.Project(_rootMapperData, (rmd, sm) => sm.GetFriendlySourcePath(rmd)).Join(" / ");

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