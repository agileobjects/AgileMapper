namespace AgileObjects.AgileMapper.ObjectPopulation.MapperKeys
{
    using Caching;

    internal struct RootMapperKeyComparer : IKeyComparer<IRootMapperKey>
    {
        public bool UseHashCodes => false;

        public bool Equals(IRootMapperKey x, IRootMapperKey y)
        {
            // ReSharper disable PossibleNullReferenceException
            return ReferenceEquals(x.RuleSet, y.RuleSet) && x.Equals(y);
            // ReSharper restore PossibleNullReferenceException
        }
    }
}