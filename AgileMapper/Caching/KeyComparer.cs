namespace AgileObjects.AgileMapper.Caching
{
    delegate bool KeyComparer<in TKey>(TKey x, TKey y);
}