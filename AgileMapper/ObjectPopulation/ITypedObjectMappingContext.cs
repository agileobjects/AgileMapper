namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Members;

    public interface ITypedObjectMappingContext<out TSource, out TTarget, out TObject>
        : ITypedMemberMappingContext<TSource, TTarget>
    {
        TObject ExistingObject { get; }

        TObject Object { get; }
    }
}