namespace AgileObjects.AgileMapper.Members
{
    internal class MappingInstanceData<TSource, TTarget> : IMappingData<TSource, TTarget>
    {
        public MappingInstanceData(
            MappingContext mappingContext,
            TSource source,
            TTarget target,
            int? enumerableIndex = null,
            IMappingData parent = null)
        {
            MappingContext = mappingContext;
            Parent = parent;
            Source = source;
            Target = target;
            EnumerableIndex = enumerableIndex;
        }

        public MappingContext MappingContext { get; }

        public IMappingData Parent { get; }

        public TSource Source { get; }

        public TTarget Target { get; set; }

        public int? EnumerableIndex { get; }
    }
}