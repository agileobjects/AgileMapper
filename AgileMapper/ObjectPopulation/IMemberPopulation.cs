namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Members;

    internal interface IMemberPopulation
    {
        IObjectMappingContext ObjectMappingContext { get; }

        Member TargetMember { get; }

        IEnumerable<Expression> NestedSourceMemberAccesses { get; }

        bool IsMultiplePopulation { get; }

        Expression Value { get; }

        bool IsSuccessful { get; }

        IMemberPopulation AddCondition(Expression condition);

        IMemberPopulation WithValue(Expression updatedValue);

        Expression GetPopulation();
    }
}