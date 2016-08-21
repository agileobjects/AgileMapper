namespace AgileObjects.AgileMapper.Members
{
    internal class MappingInstanceData<TSource, TTarget> : IMappingData<TSource, TTarget>, IMappingData
    {
        protected MappingInstanceData(IMappingData<TSource, TTarget> data)
             : this(
                   null, // <- no need for a MappingContext as we're only going to pass this to a callback
                   data.Source,
                   data.Target,
                   data.EnumerableIndex,
                   data.Parent)
        {
        }

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

        #region IMappingData Members

        T IMappingData.GetSource<T>() => (T)(object)Source;

        T IMappingData.GetTarget<T>() => (T)(object)Target;

        public int? GetEnumerableIndex() => EnumerableIndex ?? Parent?.GetEnumerableIndex();

        IMappingData<TParentSource, TParentTarget> IMappingData.As<TParentSource, TParentTarget>()
            => (IMappingData<TParentSource, TParentTarget>)this;

        #endregion
    }
}