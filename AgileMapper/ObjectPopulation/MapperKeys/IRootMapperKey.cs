namespace AgileObjects.AgileMapper.ObjectPopulation.MapperKeys
{
    internal interface IRootMapperKey : ITypedMapperKey, IRuleSetOwner
    {
        bool Equals(IRootMapperKey otherKey);
    }
}