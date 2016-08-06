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
            IMemberMappingContext context,
            DataSourceSet dataSources,
            Expression populateCondition = null)
        {
            Context = context;
            _dataSources = dataSources;
            _populateCondition = populateCondition;
        }

        #region Factory Methods

        public static IMemberPopulation IgnoredMember(IMemberMappingContext context)
            => CreateNullMemberPopulation(context, targetMember => targetMember.Name + " is ignored");

        public static IMemberPopulation NoDataSource(IMemberMappingContext context)
            => CreateNullMemberPopulation(context, targetMember => "No data source for " + targetMember.Name);

        private static IMemberPopulation CreateNullMemberPopulation(IMemberMappingContext context, Func<IQualifiedMember, string> commentFactory)
            => new MemberPopulation(
                   context,
                   new DataSourceSet(
                       new NullDataSource(
                           ReadableExpression.Comment(commentFactory.Invoke(context.TargetMember)))));

        #endregion

        public IMemberMappingContext Context { get; }

        public QualifiedMember TargetMember => Context.TargetMember;

        public bool IsSuccessful => _dataSources.HasValue;

        public Expression GetPopulation()
        {
            if (!IsSuccessful)
            {
                return _dataSources.Value;
            }

            var population = Context.TargetMember.LeafMember.GetPopulation(Context.InstanceVariable, _dataSources.Value);

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