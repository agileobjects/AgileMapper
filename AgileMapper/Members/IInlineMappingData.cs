namespace AgileObjects.AgileMapper.Members
{
    internal interface IInlineMappingData : IMappingData, IMapperDataOwner
    {
        bool TryGet<TKey, TComplex>(TKey key, out TComplex complexType);

        void Register<TKey, TComplex>(TKey key, TComplex complexType);
    }
}