namespace AgileObjects.AgileMapper
{
    using Api;

    public interface IMapper
    {
        ResultTypeSelector<TSource> Map<TSource>(TSource source);
    }
}