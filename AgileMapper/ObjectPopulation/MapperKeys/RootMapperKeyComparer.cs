namespace AgileObjects.AgileMapper.ObjectPopulation.MapperKeys
{
    using System.Collections.Generic;

    internal struct RootMapperKeyComparer : IEqualityComparer<IRootMapperKey>
    {
        public bool Equals(IRootMapperKey x, IRootMapperKey y)
        {
            // ReSharper disable PossibleNullReferenceException
            return ReferenceEquals(x.RuleSet, y.RuleSet) && x.Equals(y);
            // ReSharper restore PossibleNullReferenceException
        }

        public int GetHashCode(IRootMapperKey obj) => 0;
    }
}