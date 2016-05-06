namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using DataSources;
    using Extensions;
    using Members;
    using ReadableExpressions;

    internal class MemberPopulation : IMemberPopulation
    {
        private readonly Func<Expression, Expression> _populationFactory;
        private readonly ICollection<Expression> _conditions;

        public MemberPopulation(
            Member targetMember,
            IDataSource dataSource,
            IObjectMappingContext omc)
            : this(
                  targetMember,
                  omc.MapperContext.ValueConverters.GetConversion(dataSource.Value, targetMember.Type),
                  dataSource.NestedSourceMemberAccesses,
                  omc)
        {
        }

        private MemberPopulation(
            Member targetMember,
            Expression value,
            IEnumerable<Expression> nestedAccesses,
            IObjectMappingContext omc)
            : this(
                  targetMember,
                  value,
                  nestedAccesses,
                  finalValue => targetMember.GetPopulation(omc.TargetVariable, finalValue),
                  omc)
        {
        }

        private MemberPopulation(
            Member targetMember,
            Expression value,
            IEnumerable<Expression> nestedAccesses,
            Func<Expression, Expression> populationFactory,
            IObjectMappingContext omc)
        {
            TargetMember = targetMember;
            Value = value;
            NestedAccesses = nestedAccesses;
            _populationFactory = populationFactory;
            ObjectMappingContext = omc;
            _conditions = new List<Expression>();

            //if (CacheValueInVariable(value))
            //{
            //    _valueVariable = Expression.Variable(value.Type, targetMember.Name.ToCamelCase() + "Value");
            //    _valueVariableAssignment = Expression.Assign(_valueVariable, value);
            //    Value = _valueVariable;
            //}
            //else
            //{

            //}
        }

        //private static bool CacheValueInVariable(Expression value)
        //{
        //    return (value.NodeType == ExpressionType.Call) || (value.NodeType == ExpressionType.Invoke);
        //}

        #region Factory Methods

        public static IMemberPopulation IgnoredMember(Member targetMember, IObjectMappingContext omc)
            => CreateNullMemberPopulation(targetMember, () => targetMember.Name + " is ignored", omc);

        public static IMemberPopulation NoDataSource(Member targetMember, IObjectMappingContext omc)
            => CreateNullMemberPopulation(targetMember, () => "No data source for " + targetMember.Name, omc);

        private static IMemberPopulation CreateNullMemberPopulation(
            Member targetMember,
            Func<string> commentFactory,
            IObjectMappingContext omc)
            => new MemberPopulation(
                   targetMember,
                   Constants.EmptyExpression,
                   Enumerable.Empty<Expression>(),
                   _ => ReadableExpression.Comment(commentFactory.Invoke()),
                   omc);

        #endregion

        public Member TargetMember { get; }

        public Expression Value { get; }

        public IEnumerable<Expression> NestedAccesses { get; }

        public bool IsMultiplePopulation => false;

        public IMemberPopulation AddCondition(Expression condition)
        {
            _conditions.Add(condition);
            return this;
        }

        public Expression GetPopulation()
        {
            var population = _populationFactory.Invoke(Value);

            if (_conditions.Any())
            {
                var allConditions = _conditions.GetIsNotDefaultComparisons();

                population = Expression.IfThen(allConditions, population);
            }

            //if (_valueVariable == null)
            //{
            //    return population;
            //}

            //var populationBlock = Expression.Block(
            //    new[] { _valueVariable },
            //    _valueVariableAssignment,
            //    population,
            //    Constants.EmptyExpression);

            return population;
        }

        public bool IsSuccessful => Value != null;

        public IObjectMappingContext ObjectMappingContext { get; }

        public IMemberPopulation WithValue(Expression updatedValue)
            => new MemberPopulation(TargetMember, updatedValue, NestedAccesses, ObjectMappingContext);
    }
}