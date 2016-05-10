namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Members;

    public interface IInstanceCreationContext<out TSource, out TTarget, out TInstance>
        : ITypedMemberMappingContext<TSource, TTarget>
    {
        TInstance ExistingInstance { get; }

        TInstance CreatedInstance { get; }
    }
}