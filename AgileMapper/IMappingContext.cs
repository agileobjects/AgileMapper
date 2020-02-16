namespace AgileObjects.AgileMapper
{
    internal interface IMappingContext : IMapperContextOwner, IRuleSetOwner
    {
        bool AddUnsuccessfulMemberPopulations { get; }

        bool LazyLoadRepeatMappingFuncs { get; }
    }
}