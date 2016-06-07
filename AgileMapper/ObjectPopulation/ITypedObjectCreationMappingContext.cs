namespace AgileObjects.AgileMapper.ObjectPopulation
{
    public interface ITypedObjectCreationMappingContext<out TSource, out TTarget, out TObject>
        : ITypedObjectMappingContext<TSource, TTarget, TObject>
    {
        TObject CreatedObject { get; }
    }
}