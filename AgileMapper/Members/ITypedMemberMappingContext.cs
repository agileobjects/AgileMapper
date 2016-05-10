namespace AgileObjects.AgileMapper.Members
{
    public interface ITypedMemberMappingContext<out TSource, out TTarget>
    {
        TSource Source { get; }

        TTarget Target { get; }

        int? EnumerableIndex { get; }
    }
}