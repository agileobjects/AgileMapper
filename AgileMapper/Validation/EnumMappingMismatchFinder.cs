namespace AgileObjects.AgileMapper.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DataSources;
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;
    using ObjectPopulation;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

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
                .Project(d => EnumMappingMismatchSet.For(d.TargetMember, d.DataSources, mapperData))
                .Filter(m => m.Any)
                .ToArray();

            return mismatchSets;
        }

        public static Expression Process(Expression lambda, ObjectMapperData mapperData)
        {
            var targetMemberDatas = GetAllTargetMemberDatas(mapperData);

            if (targetMemberDatas.None())
            {
                return lambda;
            }

            var finder = new EnumMappingMismatchFinder(mapperData, targetMemberDatas);

            finder.Visit(lambda);

            var assignmentReplacements = finder._assignmentsByMismatchSet
                .SelectMany(kvp => kvp.Value.Project(assignment => new
                {
                    Assignment = assignment,
                    AssignmentWithWarning = (Expression)Expression.Block(kvp.Key.Warnings, assignment)
                }))
                .ToDictionary(d => d.Assignment, d => d.AssignmentWithWarning);

            var updatedLambda = lambda.Replace(assignmentReplacements);

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

                var dataSources = targetMemberAndDataSource
                    .Value
                    .Filter(dataSource => IsValidOtherEnumType(dataSource, targetEnumType))
                    .ToArray();

                if (dataSources.Any())
                {
                    yield return new TargetMemberData(targetMember, dataSources);
                }
            }

            var childTargetMembersAndDataSources = mapperData
                .ChildMapperDatas
                .SelectMany(EnumerateTargetMemberDatas);

            foreach (var childTargetMemberAndDataSources in childTargetMembersAndDataSources)
            {
                yield return childTargetMemberAndDataSources;
            }
        }

        private static bool IsValidOtherEnumType(IDataSource dataSource, Type targetEnumType)
        {
            return dataSource.IsValid &&
                   IsEnum(dataSource.SourceMember.Type, out var sourceEnumType) &&
                  (sourceEnumType != targetEnumType);
        }

        private static bool TargetMemberIsAnEnum(QualifiedMember targetMember, out Type targetEnumType)
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
                .FirstOrDefault(dss => dss.TargetMember.Name == memberName);

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