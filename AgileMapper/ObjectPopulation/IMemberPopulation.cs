namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Members;

    internal interface IMemberPopulation
    {
        IObjectMappingContext ObjectMappingContext { get; }

        Member TargetMember { get; }

        IEnumerable<Expression> NestedAccesses { get; }

        bool IsSuccessful { get; }

        IMemberPopulation WithCondition(Expression condition);

        Expression GetPopulation();
    }
}