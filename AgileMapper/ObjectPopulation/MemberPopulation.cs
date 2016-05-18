namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using DataSources;
    using Members;
    using ReadableExpressions;

    internal class MemberPopulation : IMemberPopulation
    {
        private readonly IMemberMappingContext _context;
        private readonly IEnumerable<IDataSource> _dataSources;
        private readonly List<ParameterExpression> _variables;
        private Expression _condition;

        public MemberPopulation(IMemberMappingContext context, IEnumerable<IDataSource> dataSources)
        {
            _context = context;
            _dataSources = dataSources;

            _variables = new List<ParameterExpression>();

            foreach (var dataSource in dataSources)
            {
                IsSuccessful = true;
                _variables.AddRange(dataSource.Variables);
            }
        }

        #region Factory Methods

        public static IMemberPopulation IgnoredMember(IMemberMappingContext context)
            => CreateNullMemberPopulation(context, targetMember => targetMember.Name + " is ignored");

        public static IMemberPopulation NoDataSource(IMemberMappingContext context)
            => CreateNullMemberPopulation(context, targetMember => "No data source for " + targetMember.Name);

        private static IMemberPopulation CreateNullMemberPopulation(IMemberMappingContext context, Func<IQualifiedMember, string> commentFactory)
            => new MemberPopulation(
                   context,
                   new[] { new NullDataSource(
                       ReadableExpression.Comment(commentFactory.Invoke(context.TargetMember))) });

        #endregion

        public IObjectMappingContext ObjectMappingContext => _context.Parent;

        public IQualifiedMember TargetMember => _context.TargetMember;

        public bool IsSuccessful { get; }

        public IEnumerable<Expression> NestedAccesses => null;

        public IMemberPopulation WithCondition(Expression condition)
        {
            _condition = condition;
            return this;
        }

        public Expression GetPopulation()
        {
            var population = _dataSources
                .Reverse()
                .Skip(1)
                .Aggregate(
                    _dataSources.Last().GetIfGuardedPopulation(_context),
                    (populationSoFar, valueProvider) => valueProvider.GetElseGuardedPopulation(populationSoFar, _context));

            if (_condition != null)
            {
                population = Expression.IfThen(_condition, population);
            }

            if (_variables.Any())
            {
                population = Expression.Block(_variables, population);
            }

            return population;
        }
    }
}