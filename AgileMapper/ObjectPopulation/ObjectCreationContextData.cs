namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Reflection;
    using Members;

    internal static class ObjectCreationContext
    {
        internal static readonly MethodInfo CreateMethod =
            typeof(ObjectCreationContext).GetMethod("Create", Constants.PublicStatic);

        public static ObjectCreationContextData<TSource, TTarget, TObject> Create<TSource, TTarget, TObject>(
            IMappingData<TSource, TTarget> data,
            TObject createdObject)
        {
            return new ObjectCreationContextData<TSource, TTarget, TObject>(data, createdObject);
        }
    }

    internal class ObjectCreationContextData<TSource, TTarget, TObject> :
        MappingInstanceData<TSource, TTarget>,
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