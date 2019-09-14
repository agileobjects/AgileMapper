namespace AgileObjects.AgileMapper.Configuration.MemberIgnores.SourceValueFilters
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal static class FilterConstants
    {
        public static readonly Expression True = Expression.Constant(true, typeof(bool));
        public static readonly Expression False = Expression.Constant(false, typeof(bool));
    }
}