namespace AgileObjects.AgileMapper
{
    internal interface IMappingContext : IMapperContextOwner, IRuleSetOwner
    {
        bool IncludeCodeComments { get; }

        bool IgnoreUnsuccessfulMemberPopulations { get; }

        bool LazyLoadRepeatMappingFuncs { get; }
    }
}