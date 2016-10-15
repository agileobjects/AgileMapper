namespace AgileObjects.AgileMapper.Members
{
    internal abstract class MappingInstanceDataBase<TSource, TTarget> : IMappingData<TSource, TTarget>
    {
        protected MappingInstanceDataBase(IMappingData<TSource, TTarget> mappingData)
            : this(mappingData.Source, mappingData.Target, mappingData.EnumerableIndex, mappingData.Parent)
        {
        }

        protected MappingInstanceDataBase(
            TSource source,
            TTarget target,
            int? enumerableIndex,
            IMappingData parent)
        {
            Parent = parent;
            Source = source;
            Target = target;
            EnumerableIndex = enumerableIndex;
        }

        public IMappingData Parent { get; }

        public TSource Source { get; }

        public TTarget Target { get; set; }

        public int? EnumerableIndex { get; }
    }
}