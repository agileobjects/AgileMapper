namespace AgileObjects.AgileMapper.Members
{
    internal class MappingInstanceData<TSource, TTarget> : IMappingData<TSource, TTarget>, IMappingData
    {
        private readonly IMappingData _parent;

        protected MappingInstanceData(IMappingData<TSource, TTarget> mappingData)
            : this(mappingData.Source, mappingData.Target, mappingData.EnumerableIndex, mappingData.Parent)
        {
        }

        protected MappingInstanceData(
            TSource source,
            TTarget target,
            int? enumerableIndex,
            IMappingData parent)
        {
            _parent = parent;
            Source = source;
            Target = target;
            EnumerableIndex = enumerableIndex;
        }

        IMappingData IMappingData.Parent => _parent;

        IMappingData IMappingData<TSource, TTarget>.Parent => _parent;

        public TSource Source { get; }

        public TTarget Target { get; set; }

        public int? EnumerableIndex { get; }

        T IMappingData.GetSource<T>() => Source as T;

        T IMappingData.GetTarget<T>() => Target as T;

        public int? GetEnumerableIndex() => EnumerableIndex ?? _parent?.GetEnumerableIndex();

        IMappingData<TDataSource, TDataTarget> IMappingData.As<TDataSource, TDataTarget>()
        {
            var thisMappingData = (IMappingData)this;

            return new MappingInstanceData<TDataSource, TDataTarget>(
                thisMappingData.GetSource<TDataSource>(),
                thisMappingData.GetSource<TDataTarget>(),
                GetEnumerableIndex(),
                _parent);
        }
    }
}