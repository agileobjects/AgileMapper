namespace AgileObjects.AgileMapper.Caching
{
    using System.Collections.Generic;

    internal interface IKeyComparer<TKey> : IEqualityComparer<TKey>
    {
        bool UseHashCodes { get; }
    }
}