namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Members;

    public interface IObjectCreationMappingData<out TSource, TTarget, out TObject> : IMappingData<TSource, TTarget>
    {
        TObject CreatedObject { get; }
    }
}