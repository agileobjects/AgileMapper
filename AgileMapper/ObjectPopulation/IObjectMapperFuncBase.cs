namespace AgileObjects.AgileMapper.ObjectPopulation
{
    internal interface IObjectMapperFuncBase
    {
        object Map(object source, object target, IMappingExecutionContext context);
    }
}