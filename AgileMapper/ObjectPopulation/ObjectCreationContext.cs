namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Reflection;
    using Members;

    internal static class ObjectCreationContext
    {
        internal static readonly MethodInfo CreateMethod =
            typeof(ObjectCreationContext).GetMethod("Create", Constants.PublicStatic);

        public static ObjectCreationContext<TSource, TTarget, TObject> Create<TSource, TTarget, TObject>(
            ITypedMemberMappingContext<TSource, TTarget> memberMappingContext,
            TObject createdCbject)
        {
            return new ObjectCreationContext<TSource, TTarget, TObject>(memberMappingContext, createdCbject);
        }
    }

    internal class ObjectCreationContext<TSource, TTarget, TObject> :
        TypedMemberMappingContext<TSource, TTarget>,
        IObjectCreationContext<TSource, TTarget, TObject>
    {
        public ObjectCreationContext(
            ITypedMemberMappingContext<TSource, TTarget> memberMappingContext,
            TObject createdObject)
            : base(memberMappingContext.Source, memberMappingContext.Target, memberMappingContext.EnumerableIndex)
        {
            CreatedObject = createdObject;
        }

        public TObject CreatedObject { get; }
    }
}