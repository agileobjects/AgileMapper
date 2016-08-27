namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using Members;

    public interface IConditionalMappingConfigurator<TSource, TTarget> : IRootMappingConfigurator<TSource, TTarget>
    {
        IConditionalRootMappingConfigurator<TSource, TTarget> If(
            Expression<Func<IMappingData<TSource, TTarget>, bool>> condition);

        IConditionalRootMappingConfigurator<TSource, TTarget> If(
            Expression<Func<TSource, TTarget, bool>> condition);

        IConditionalRootMappingConfigurator<TSource, TTarget> If(
            Expression<Func<TSource, TTarget, int?, bool>> condition);
    }
}