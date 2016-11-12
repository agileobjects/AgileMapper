namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Reflection;
    using Members;
    using NetStandardPolyfills;

    internal static class ObjectCreationMappingData
    {
        internal static readonly MethodInfo CreateMethod =
            typeof(ObjectCreationMappingData).GetPublicStaticMethod("Create");

        public static ObjectCreationMappingData<TSource, TTarget, TObject> Create<TSource, TTarget, TObject>(
            IMappingData<TSource, TTarget> mappingData,
            TObject createdCbject)
        {
            return new ObjectCreationMappingData<TSource, TTarget, TObject>(mappingData, createdCbject);
        }
    }

    internal class ObjectCreationMappingData<TSource, TTarget, TObject> :
        MappingInstanceData<TSource, TTarget>,
        IObjectCreationMappingData<TSource, TTarget, TObject>
    {
        private readonly TObject _createdObject;

        public ObjectCreationMappingData(IMappingData<TSource, TTarget> mappingData, TObject createdObject)
            : base(mappingData)
        {
            _createdObject = createdObject;
        }

        TObject IObjectCreationMappingData<TSource, TTarget, TObject>.CreatedObject => _createdObject;
    }
}