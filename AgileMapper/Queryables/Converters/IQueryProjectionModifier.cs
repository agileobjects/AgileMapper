namespace AgileObjects.AgileMapper.Queryables.Converters
{
    using Members;
    using Settings;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal interface IQueryProjectionModifier
    {
        IQueryProviderSettings Settings { get; }

        IMemberMapperData MapperData { get; }

        Expression Modify(Expression queryProjection);
    }
}