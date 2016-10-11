namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Reflection;
    using Members;
    using Extensions;

    internal static class ObjectCreationContextData
    {
        internal static readonly MethodInfo CreateMethod =
            typeof(ObjectCreationContextData).GetPublicStaticMethod("Create");

        public static ObjectCreationContextData<TSource, TTarget, TObject> Create<TSource, TTarget, TObject>(
            IMappingData<TSource, TTarget> data,
            TObject createdCbject)
        {
            return new ObjectCreationContextData<TSource, TTarget, TObject>(data, createdCbject);
        }
    }

    internal class ObjectCreationContextData<TSource, TTarget, TObject> :
        MappingInstanceDataBase<TSource, TTarget>,
        IObjectCreationMappingData<TSource, TTarget, TObject>
    {
        private readonly TObject _createdObject;

        public ObjectCreationContextData(IMappingData<TSource, TTarget> data, TObject createdObject)
            : base(data)
        {
            _createdObject = createdObject;
        }

        TObject IObjectCreationMappingData<TSource, TTarget, TObject>.CreatedObject => _createdObject;
    }
}