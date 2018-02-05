namespace AgileObjects.AgileMapper.Queryables.Converters
{
    using System.Linq.Expressions;
    using Members;
    using Settings;

    internal interface IQueryProjectionModifier
    {
        IQueryProviderSettings Settings { get; }

        IMemberMapperData MapperData { get; }

        Expression Modify(Expression queryProjection);
    }
}