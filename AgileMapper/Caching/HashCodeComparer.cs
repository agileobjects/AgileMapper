namespace AgileObjects.AgileMapper.Caching
{
    internal struct HashCodeComparer<T> : IKeyComparer<T>
    {
        public bool UseHashCodes => true;

        public bool Equals(T x, T y) => x.GetHashCode() == y.GetHashCode();

        public int GetHashCode(T obj) => obj.GetHashCode();
    }
}