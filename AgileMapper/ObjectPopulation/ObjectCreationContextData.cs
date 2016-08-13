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
            TObject createdCbject)
        {
            return new ObjectCreationContextData<TSource, TTarget, TObject>(data, createdCbject);
        }
    }

    internal class ObjectCreationContextData<TSource, TTarget, TObject> :
        MappingInstanceData<TSource, TTarget>,
        IObjectCreationMappingData<TSource, TTarget, TObject>
    {
        private readonly TObject _createdObject;

        public ObjectCreationContextData(IMappingData<TSource, TTarget> data, TObject createdObject)
            : base(
                  null, // <- no need for a MappingContext as we're only passing this to a creation callback
                  data.Source,
                  data.Target,
                  data.EnumerableIndex,
                  data.Parent)
        {
            _createdObject = createdObject;
        }

        TObject IObjectCreationMappingData<TSource, TTarget, TObject>.CreatedObject => _createdObject;
    }
}