namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;
    using Members;

    internal interface IMemberPopulation
    {
        IObjectMappingContext ObjectMappingContext { get; }

        QualifiedMember TargetMember { get; }

        bool IsSuccessful { get; }

        Expression GetPopulation();
    }
}