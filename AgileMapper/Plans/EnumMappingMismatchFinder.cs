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
        private readonly TargetMemberAndDataSources[] _dataSourcesByTargetMember;
        private readonly Dictionary<Expression, Expression> _assignmentReplacements;

        private EnumMappingMismatchFinder(
            ObjectMapperData mapperData,
            TargetMemberAndDataSources[] dataSourcesByTargetMember)
        {
            _mapperData = mapperData;
            _dataSourcesByTargetMember = dataSourcesByTargetMember;
            _assignmentReplacements = new Dictionary<Expression, Expression>();
        }

        public static Expression Process(Expression lambda, IObjectMappingData mappingData)
        {
            var dataSourcesByTargetMember = GetDataSourcesByTargetMember(mappingData);

            if (dataSourcesByTargetMember.None())
            {
                return lambda;
            }

            var finder = new EnumMappingMismatchFinder(
                mappingData.MapperData,
                dataSourcesByTargetMember);

            finder.Visit(lambda);

            var updatedLambda = lambda.Replace(finder._assignmentReplacements);

            return updatedLambda;
        }

        private static TargetMemberAndDataSources[] GetDataSourcesByTargetMember(
            IObjectMappingData mappingData)
        {
            return GetAllDataSourcesByTargetMember(mappingData.MapperData)
                .Select(d => new TargetMemberAndDataSources(d.Item1, d.Item2))
                .ToArray();
        }

        private static IEnumerable<Tuple<QualifiedMember, DataSourceSet>> GetAllDataSourcesByTargetMember(
            ObjectMapperData mapperData)
        {
            foreach (var targetMemberAndDataSourceSet in mapperData.DataSourcesByTargetMember)
            {
                var targetMember = targetMemberAndDataSourceSet.Key;

                if (!TargetMemberIsAnEnum(targetMember))
                {
                    continue;
                }

                var dataSources = targetMemberAndDataSourceSet.Value
                    .Where(dataSource => IsEnum(dataSource.Value.Type));

                if (dataSources.Any())
                {
                    yield return Tuple.Create(targetMember, targetMemberAndDataSourceSet.Value);
                }
            }

            var childTargetMembersAndDataSources = mapperData
                .ChildMapperDatas
                .SelectMany(GetAllDataSourcesByTargetMember);

            foreach (var childTargetMemberAndDataSources in childTargetMembersAndDataSources)
            {
                yield return childTargetMemberAndDataSources;
            }
        }

        private static bool TargetMemberIsAnEnum(QualifiedMember targetMember)
            => targetMember.IsSimple && IsEnum(targetMember.Type);

        private static bool IsEnum(Type type) => type.GetNonNullableUnderlyingTypeIfAppropriate().IsEnum();

        protected override Expression VisitBinary(BinaryExpression binary)
        {
            TargetMemberAndDataSources targetMemberAndDataSources;

            if ((binary.NodeType == ExpressionType.Assign) &&
                IsEnum(binary.Left.Type) &&
                TryGetMatch(binary.Right, out targetMemberAndDataSources))
            {
                var mismatchWarnings = EnumMappingMismatchSet.For(
                    targetMemberAndDataSources.TargetMember,
                    targetMemberAndDataSources.DataSources.Where(ds => IsEnum(ds.SourceMember.Type)),
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

        private bool TryGetMatch(Expression value, out TargetMemberAndDataSources targetMemberAndDataSources)
        {
            targetMemberAndDataSources = _dataSourcesByTargetMember
                .FirstOrDefault(dss => dss.DataSources.Value == value);

            return targetMemberAndDataSources != null;
        }

        #region Helper Class

        private class TargetMemberAndDataSources
        {
            public TargetMemberAndDataSources(QualifiedMember targetMember, DataSourceSet dataSources)
            {
                TargetMember = targetMember;
                DataSources = dataSources;
            }

            public QualifiedMember TargetMember { get; }

            public DataSourceSet DataSources { get; }
        }

        #endregion
    }
}