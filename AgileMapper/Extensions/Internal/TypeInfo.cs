namespace AgileObjects.AgileMapper.Extensions.Internal
{
    internal static class TypeInfo<T>
    {
        public static readonly bool IsEnumerable = typeof(T).IsEnumerable();
        public static readonly bool RuntimeTypeNeeded = typeof(T).RuntimeTypeNeeded();
    }
}