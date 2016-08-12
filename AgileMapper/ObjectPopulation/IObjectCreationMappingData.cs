namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Members;

    public interface IObjectCreationMappingData<out TSource, out TTarget, out TObject> : IMappingData<TSource, TTarget>
    {
        TObject CreatedObject { get; }
    }
}