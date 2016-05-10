namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Members;

    internal class InstanceCreationContext<TSource, TTarget, TInstance> :
        TypedMemberMappingContext<TSource, TTarget>,
        IInstanceCreationContext<TSource, TTarget, TInstance>
    {
        internal InstanceCreationContext(
            TSource source,
            TTarget target,
            TInstance existingInstance,
            int? enumerableIndex)
            : base(source, target, enumerableIndex)
        {
            ExistingInstance = existingInstance;
        }

        public TInstance ExistingInstance { get; }

        public TInstance CreatedInstance { get; set; }
    }
}