namespace AgileObjects.AgileMapper.Members
{
    internal class MappingInstanceData<TSource, TTarget> : IMappingData<TSource, TTarget>, IMappingData
    {
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
            Parent = parent;
            Source = source;
            Target = target;
            EnumerableIndex = enumerableIndex;
        }

        public IMappingData Parent { get; }

        public TSource Source { get; }

        public TTarget Target { get; set; }

        public int? EnumerableIndex { get; }

        T IMappingData.GetSource<T>() => Source as T;

        T IMappingData.GetTarget<T>() => Target as T;

        public int? GetEnumerableIndex() => EnumerableIndex ?? Parent?.GetEnumerableIndex();

        IMappingData<TDataSource, TDataTarget> IMappingData.As<TDataSource, TDataTarget>()
        {
            var thisMappingData = (IMappingData)this;

            return new MappingInstanceData<TDataSource, TDataTarget>(
                thisMappingData.GetSource<TDataSource>(),
                thisMappingData.GetSource<TDataTarget>(),
                GetEnumerableIndex(),
                Parent);
        }
    }
}