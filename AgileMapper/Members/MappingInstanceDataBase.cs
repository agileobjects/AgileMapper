namespace AgileObjects.AgileMapper.Members
{
    internal abstract class MappingInstanceDataBase<TSource, TTarget> : IMappingData<TSource, TTarget>, IMappingData
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

        T IMappingData.GetSource<T>() => (T)(object)Source;

        T IMappingData.GetTarget<T>() => (T)(object)Target;

        public int? GetEnumerableIndex() => EnumerableIndex ?? Parent?.GetEnumerableIndex();

        IMappingData<TDataSource, TDataTarget> IMappingData.As<TDataSource, TDataTarget>()
            => (IMappingData<TDataSource, TDataTarget>)this;
    }
}