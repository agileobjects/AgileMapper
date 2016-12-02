namespace AgileObjects.AgileMapper.Plans
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using DataSources;
    using Extensions;
    using Members;
    using NetStandardPolyfills;
    using ObjectPopulation;

    internal class EnumMappingMismatchFinder : ExpressionVisitor
    {
        private readonly ObjectMapperData _mapperData;
        private readonly TargetMemberData[] _targetMemberDatas;
        private readonly Dictionary<Expression, Expression> _assignmentReplacements;

        private EnumMappingMismatchFinder(
            ObjectMapperData mapperData,
            TargetMemberData[] targetMemberDatas)
        {
            _mapperData = mapperData;
            _targetMemberDatas = targetMemberDatas;
            _assignmentReplacements = new Dictionary<Expression, Expression>();
        }

        public static Expression Process(Expression lambda, IObjectMappingData mappingData)
        {
            var targetMemberDatas = GetAllTargetMemberDatas(mappingData);

            if (targetMemberDatas.None())
            {
                return lambda;
            }

            var finder = new EnumMappingMismatchFinder(
                mappingData.MapperData,
                targetMemberDatas);

            finder.Visit(lambda);

            var updatedLambda = lambda.Replace(finder._assignmentReplacements);

            return updatedLambda;
        }

        private static TargetMemberData[] GetAllTargetMemberDatas(IObjectMappingData mappingData)
            => EnumerateTargetMemberDatas(mappingData.MapperData).ToArray();

        private static IEnumerable<TargetMemberData> EnumerateTargetMemberDatas(ObjectMapperData mapperData)
        {
            foreach (var targetMemberAndDataSource in mapperData.DataSourcesByTargetMember)
            {
                var targetMember = targetMemberAndDataSource.Key;

                if (!TargetMemberIsAnEnum(targetMember))
                {
                    continue;
                }

                var dataSources = targetMemberAndDataSource.Value
                    .Where(dataSource => dataSource.IsValid && IsEnum(dataSource.SourceMember.Type))
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

        private static bool TargetMemberIsAnEnum(QualifiedMember targetMember)
            => targetMember.IsSimple && IsEnum(targetMember.Type);

        private static bool IsEnum(Type type) => type.GetNonNullableType().IsEnum();

        protected override Expression VisitBinary(BinaryExpression binary)
        {
            TargetMemberData targetMemberData;

            if ((binary.NodeType == ExpressionType.Assign) &&
                IsEnum(binary.Left.Type) &&
                TryGetMatch(binary.Left, out targetMemberData))
            {
                var mismatchWarnings = EnumMappingMismatchSet.For(
                    targetMemberData.TargetMember,
                    targetMemberData.DataSources,
                    _mapperData);

                if (mismatchWarnings.Any)
                {
                    _assignmentReplacements.Add(
                        binary,
                        Expression.Block(mismatchWarnings.Warnings, binary));
                }
            }

            return base.VisitBinary(binary);
        }

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