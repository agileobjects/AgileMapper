namespace AgileObjects.AgileMapper.Caching
{
    internal interface IKeyComparer<in TKey>
    {
        bool UseHashCodes { get; }

        bool Equals(TKey x, TKey y);
    }
}