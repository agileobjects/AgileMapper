namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using DataSources;
    using Members;
    using ReadableExpressions;

    internal class MemberPopulation : IMemberPopulation
    {
        private readonly DataSourceSet _dataSources;
        private readonly Expression _populateCondition;

        public MemberPopulation(
            MemberMapperData mapperData,
            DataSourceSet dataSources,
            Expression populateCondition = null)
        {
            MapperData = mapperData;
            _dataSources = dataSources;
            _populateCondition = populateCondition;

            if (!mapperData.TargetMember.IsSimple)
            {
                mapperData.Parent.RegisterTargetMemberDataSources(mapperData.TargetMember, dataSources);
            }
        }

        #region Factory Methods

        public static IMemberPopulation IgnoredMember(MemberMapperData mapperData)
            => CreateNullMemberPopulation(mapperData, targetMember => targetMember.Name + " is ignored");

        public static IMemberPopulation NoDataSource(MemberMapperData mapperData)
            => CreateNullMemberPopulation(mapperData, targetMember => "No data source for " + targetMember.Name);

        private static IMemberPopulation CreateNullMemberPopulation(
            MemberMapperData mapperData,
            Func<IQualifiedMember, string> commentFactory)
        {
            return new MemberPopulation(
                mapperData,
                new DataSourceSet(
                    new NullDataSource(
                        ReadableExpression.Comment(commentFactory.Invoke(mapperData.TargetMember)))));
        }

        #endregion

        public MemberMapperData MapperData { get; }

        public bool IsSuccessful => _dataSources.HasValue;

        public Expression SourceMemberTypeTest => _dataSources.SourceMemberTypeTest;

        public Expression GetPopulation()
        {
            if (!IsSuccessful)
            {
                return _dataSources.Value;
            }

            var population = MapperData.TargetMember.LeafMember.GetPopulation(MapperData.InstanceVariable, _dataSources.Value);

            if (_dataSources.Variables.Any())
            {
                population = Expression.Block(_dataSources.Variables, population);
            }

            if (_populateCondition != null)
            {
                population = Expression.IfThen(_populateCondition, population);
            }

            return population;
        }
    }
}