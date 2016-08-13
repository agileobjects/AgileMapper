namespace AgileObjects.AgileMapper.Members
{
    public interface IMappingData
    {
        IMappingData Parent { get; }

        TSource GetSource<TSource>();

        TTarget GetTarget<TTarget>();

        int? GetEnumerableIndex();

        IMappingData<TParentSource, TParentTarget> As<TParentSource, TParentTarget>();
    }

    public interface IMappingData<out TSource, out TTarget>
    {
        IMappingData Parent { get; }

        TSource Source { get; }

        TTarget Target { get; }

        int? EnumerableIndex { get; }
    }
}