namespace AgileObjects.AgileMapper.Members
{
    using System.Reflection;

    internal static class TypedMemberMappingContext
    {
        public static readonly MethodInfo CreateMethod =
            typeof(TypedMemberMappingContext).GetMethod("Create", Constants.PublicStatic);

        public static ITypedMemberMappingContext<TSource, TTarget> Create<TSource, TTarget>(
            TSource source,
            TTarget target,
            int? enumerableIndex)
        {
            return new TypedMemberMappingContext<TSource, TTarget>(source, target, enumerableIndex);
        }
    }

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