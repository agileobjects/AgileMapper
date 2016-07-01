namespace AgileObjects.AgileMapper.Members
{
    internal class TypedMemberMappingContext<TSource, TTarget> : ITypedMemberMappingContext<TSource, TTarget>
    {
        internal TypedMemberMappingContext(TSource source, TTarget target, int? enumerableIndex)
        {
            Source = source;
            Target = target;
            EnumerableIndex = enumerableIndex;
        }

        public TSource Source { get; }

        public TTarget Target { get; set; }

        public int? EnumerableIndex { get; }
    }
}