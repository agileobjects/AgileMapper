namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;
    using Members;

    internal interface IMemberPopulation
    {
        IObjectMappingContext ObjectMappingContext { get; }

        IQualifiedMember TargetMember { get; }

        bool IsSuccessful { get; }

        IMemberPopulation WithCondition(Expression condition);

        Expression GetPopulation();
    }
}