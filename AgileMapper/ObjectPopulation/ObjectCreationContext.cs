namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Reflection;
    using Members;

    internal static class ObjectCreationContext
    {
        internal static readonly MethodInfo CreateMethod =
            typeof(ObjectCreationContext).GetMethod("Create", Constants.PublicStatic);

        public static ObjectCreationContext<TSource, TTarget, TObject> Create<TSource, TTarget, TObject>(
            TSource source,
            TTarget target,
            TObject createdCbject,
            int? enumerableIndex)
        {
            return new ObjectCreationContext<TSource, TTarget, TObject>(source, target, createdCbject, enumerableIndex);
        }
    }

    internal class ObjectCreationContext<TSource, TTarget, TObject> :
        TypedMemberMappingContext<TSource, TTarget>,
        IObjectCreationContext<TSource, TTarget, TObject>
    {
        internal ObjectCreationContext(
            TSource source,
            TTarget target,
            TObject createdObject,
            int? enumerableIndex)
            : base(source, target, enumerableIndex)
        {
            CreatedObject = createdObject;
        }

        public TObject CreatedObject { get; }
    }
}