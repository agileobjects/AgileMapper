namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using Members;

    internal interface IObjectMappingData : IMappingData
    {
        IMappingContext MappingContext { get; }

        bool IsRoot { get; }

        new IObjectMappingData Parent { get; }

        bool IsPartOfDerivedTypeMapping { get; }

        IObjectMappingData DeclaredTypeMappingData { get; }

        ObjectMapperKeyBase MapperKey { get; }

        ObjectMapperData MapperData { get; }

        IObjectMapper Mapper { get; set; }

        IMemberMappingData GetChildMappingData(IMemberMapperData childMapperData);

        object MapStart();

        TDeclaredTarget MapRecursion<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource sourceValue,
            TDeclaredTarget targetValue,
            string targetMemberName,
            int dataSourceIndex);

        bool TryGet<TKey, TComplex>(TKey key, out TComplex complexType);

        void Register<TKey, TComplex>(TKey key, TComplex complexType);

        IObjectMappingData WithTypes(Type newSourceType, Type newTargetType);
    }
}