namespace AgileObjects.AgileMapper
{
    internal interface IMappingContext : IMapperContextOwner, IRuleSetOwner
    {
        bool IgnoreUnsuccessfulMemberPopulations { get; }

        bool LazyLoadRepeatMappingFuncs { get; }
    }
}