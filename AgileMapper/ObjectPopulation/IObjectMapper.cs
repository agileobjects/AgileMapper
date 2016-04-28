namespace AgileObjects.AgileMapper.ObjectPopulation
{
    internal interface IObjectMapper<out TTarget>
    {
        TTarget Execute(IObjectMappingContext objectMappingContext);
    }
}