namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;
    using Members;

    internal interface IMemberPopulation
    {
        IObjectMappingContext ObjectMappingContext { get; }

        IQualifiedMember TargetMember { get; }

        bool IsSuccessful { get; }

        void AddCondition(Expression condition);

        Expression GetPopulation();
    }
}