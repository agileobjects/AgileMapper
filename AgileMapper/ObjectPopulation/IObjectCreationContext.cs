namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Members;

    public interface IObjectCreationContext<out TSource, out TTarget, out TObject>
        : ITypedMemberMappingContext<TSource, TTarget>
    {
        TObject CreatedObject { get; }
    }
}