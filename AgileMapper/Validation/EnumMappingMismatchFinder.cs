namespace AgileObjects.AgileMapper.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Caching.Dictionaries;
    using DataSources;
    using Extensions;
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;
    using ObjectPopulation;
    using ReadableExpressions.Extensions;

    internal class EnumMappingMismatchFinder : ExpressionVisitor
    {
        private readonly ObjectMapperData _mapperData;
        private readonly TargetMemberData[] _targetMemberDatas;
        private readonly Dictionary<EnumMappingMismatchSet, List<Expression>> _assignmentsByMismatchSet;

        private EnumMappingMismatchFinder(
            ObjectMapperData mapperData,
            TargetMemberData[] targetMemberDatas)
        {
            _mapperData = mapperData;
            _targetMemberDatas = targetMemberDatas;

            _assignmentsByMismatchSet =
                new Dictionary<EnumMappingMismatchSet, List<Expression>>(EnumMappingMismatchSet.Comparer);
        }

        public static ICollection<EnumMappingMismatchSet> FindMismatches(ObjectMapperData mapperData)
        {
            var targetMemberDatas = GetAllTargetMemberDatas(mapperData);

            if (targetMemberDatas.None())
            {
                return Enumerable<EnumMappingMismatchSet>.EmptyArray;
            }

            var mismatchSets = targetMemberDatas
                .Project(mapperData, (md, d) => EnumMappingMismatchSet.For(d.TargetMember, d.DataSources, md))
                .Filter(m => m.Any)
                .ToArray();

            return mismatchSets;
        }

        public static LambdaExpression Process(LambdaExpression mapping, ObjectMapperData mapperData)
        {
            var targetMemberDatas = GetAllTargetMemberDatas(mapperData);

            if (targetMemberDatas.None())
            {
                return mapping;
            }

            var finder = new EnumMappingMismatchFinder(mapperData, targetMemberDatas);

            finder.Visit(mapping);

            var assignmentData = finder._assignmentsByMismatchSet
                .SelectMany(kvp => kvp.Value.Project(kvp.Key, (k, assignment) => new
                {
                    Assignment = assignment,
                    AssignmentWithWarning = (Expression)Expression.Block(k.Warnings, assignment)
                }))
                .ToArray();

            var assignmentsCount = assignmentData.Length;

            if (assignmentsCount == 1)
            {
                var singleData = assignmentData[0];

                return mapping.Replace(singleData.Assignment, singleData.AssignmentWithWarning);
            }

            var assignmentReplacements = FixedSizeExpressionReplacementDictionary
                .WithEqualKeys(assignmentsCount);

            for (var i = 0; i < assignmentsCount; ++i)
            {
                var data = assignmentData[i];

                assignmentReplacements.Add(data.Assignment, data.AssignmentWithWarning);
            }

            var updatedLambda = mapping.Replace(assignmentReplacements);

            return updatedLambda;
        }

        private static TargetMemberData[] GetAllTargetMemberDatas(ObjectMapperData mapperData)
            => EnumerateTargetMemberDatas(mapperData).ToArray();

        private static IEnumerable<TargetMemberData> EnumerateTargetMemberDatas(ObjectMapperData mapperData)
        {
            foreach (var targetMemberAndDataSource in mapperData.DataSourcesByTargetMember)
            {
                var targetMember = targetMemberAndDataSource.Key;

                if (!TargetMemberIsAnEnum(targetMember, out var targetEnumType))
                {
                    continue;
                }

                var dataSources = targetMemberAndDataSource.Value;
                var validDataSources = default(List<IDataSource>);

                for (var i = 0; i < dataSources.Count; i++)
                {
                    var dataSource = dataSources[i];

                    if (!IsValidOtherEnumType(targetEnumType, dataSource))
                    {
                        continue;
                    }

                    if (validDataSources == null)
                    {
                        validDataSources = new List<IDataSource>();
                    }

                    validDataSources.Add(dataSource);
                }

                if (validDataSources?.Any() == true)
                {
                    yield return new TargetMemberData(targetMember, validDataSources);
                }
            }

            var childTargetMembersAndDataSources = mapperData
                .ChildMapperDatasOrEmpty
                .SelectMany(EnumerateTargetMemberDatas);

            foreach (var childTargetMemberAndDataSources in childTargetMembersAndDataSources)
            {
                yield return childTargetMemberAndDataSources;
            }
        }

        private static bool IsValidOtherEnumType(Type targetEnumType, IDataSource dataSource)
        {
            return dataSource.IsValid &&
                   IsEnum(dataSource.SourceMember.Type, out var sourceEnumType) &&
                  (sourceEnumType != targetEnumType);
        }

        private static bool TargetMemberIsAnEnum(IQualifiedMember targetMember, out Type targetEnumType)
        {
            if (targetMember.IsSimple && IsEnum(targetMember.Type, out targetEnumType))
            {
                return true;
            }

            targetEnumType = null;
            return false;
        }

        private static bool IsEnum(Type type, out Type enumType) => (enumType = type.GetNonNullableType()).IsEnum();

        protected override Expression VisitBinary(BinaryExpression binary)
        {
            if ((binary.NodeType == ExpressionType.Assign) &&
                (binary.Left.NodeType != ExpressionType.Parameter) &&
                 IsEnum(binary.Left.Type) &&
                 TryGetMatch(binary.Left, out var targetMemberData))
            {
                var mismatchWarnings = EnumMappingMismatchSet.For(
                    targetMemberData.TargetMember,
                    targetMemberData.DataSources,
                    _mapperData);

                if (mismatchWarnings.Any)
                {
                    if (!_assignmentsByMismatchSet.TryGetValue(mismatchWarnings, out var assignments))
                    {
                        _assignmentsByMismatchSet.Add(mismatchWarnings, (assignments = new List<Expression>()));
                    }

                    assignments.Add(binary);
                }
            }

            return base.VisitBinary(binary);
        }

        private static bool IsEnum(Type type) => IsEnum(type, out _);

        private bool TryGetMatch(Expression targetMemberAccess, out TargetMemberData targetMemberData)
        {
            var memberName = targetMemberAccess.GetMemberName();

            targetMemberData = _targetMemberDatas
                .FirstOrDefault(memberName, (mn, dss) => dss.TargetMember.Name == mn);

            return targetMemberData != null;
        }

        #region Helper Class

        private class TargetMemberData
        {
            public TargetMemberData(QualifiedMember targetMember, IEnumerable<IDataSource> dataSources)
            {
                TargetMember = targetMember;
                DataSources = dataSources;
            }

            public QualifiedMember TargetMember { get; }

            public IEnumerable<IDataSource> DataSources { get; }
        }

        #endregion
    }
}