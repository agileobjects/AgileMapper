namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Members;
    using ReadableExpressions;

    internal class MemberPopulation : IMemberPopulation
    {
        private readonly IMemberMappingContext _context;
        private readonly IEnumerable<ValueProvider> _valueProviders;
        private readonly List<ParameterExpression> _variables;
        private Expression _condition;

        public MemberPopulation(IMemberMappingContext context, IEnumerable<ValueProvider> valueProviders)
        {
            _context = context;
            _valueProviders = valueProviders;
            TargetMember = context.TargetMember.LeafMember;

            _variables = new List<ParameterExpression>();

            foreach (var valueProvider in valueProviders)
            {
                _variables.AddRange(valueProvider.Variables);

                if (valueProvider.IsSuccessful)
                {
                    IsSuccessful = true;
                }
            }
        }

        #region Factory Methods

        public static IMemberPopulation IgnoredMember(IMemberMappingContext context)
            => CreateNullMemberPopulation(context, targetMember => targetMember.Name + " is ignored");

        public static IMemberPopulation NoDataSource(IMemberMappingContext context)
            => CreateNullMemberPopulation(context, targetMember => "No data source for " + targetMember.Name);

        private static IMemberPopulation CreateNullMemberPopulation(IMemberMappingContext context, Func<Member, string> commentFactory)
            => new MemberPopulation(
                   context,
                   new[]
                   {
                       ValueProvider.Null(ctx => ReadableExpression
                           .Comment(commentFactory.Invoke(ctx.TargetMember.LeafMember)))
                   });

        #endregion

        public IObjectMappingContext ObjectMappingContext => _context.Parent;

        public Member TargetMember { get; }

        public bool IsSuccessful { get; }

        public IEnumerable<Expression> NestedAccesses => null;

        public IMemberPopulation WithCondition(Expression condition)
        {
            _condition = condition;
            return this;
        }

        public Expression GetPopulation()
        {
            var population = _valueProviders
                .Reverse()
                .Skip(1)
                .Aggregate(
                    _valueProviders.Last().GetIfGuardedPopulation(_context),
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