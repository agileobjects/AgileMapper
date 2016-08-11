namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using Members;

    public interface IRootMappingConfigurator<TSource, TTarget>
    {
        void CreateInstancesUsing(Expression<Func<IMappingData<TSource, TTarget>, TTarget>> factory);

        void CreateInstancesUsing<TFactory>(TFactory factory) where TFactory : class;

        IFactorySpecifier<TSource, TTarget, TObject> CreateInstancesOf<TObject>() where TObject : class;

        MappingConfigContinuation<TSource, TTarget> Ignore(params Expression<Func<TTarget, object>>[] targetMembers);

        CustomDataSourceTargetMemberSpecifier<TSource, TTarget> Map<TSourceValue>(
            Expression<Func<IMappingData<TSource, TTarget>, TSourceValue>> valueFactoryExpression);

        CustomDataSourceTargetMemberSpecifier<TSource, TTarget> Map<TSourceValue>(
            Expression<Func<TSource, TTarget, TSourceValue>> valueFactoryExpression);

        CustomDataSourceTargetMemberSpecifier<TSource, TTarget> Map<TSourceValue>(
            Expression<Func<TSource, TTarget, int?, TSourceValue>> valueFactoryExpression);

        CustomDataSourceTargetMemberSpecifier<TSource, TTarget> MapFunc<TSourceValue>(
            Func<TSource, TSourceValue> valueFunc);

        CustomDataSourceTargetMemberSpecifier<TSource, TTarget> Map<TSourceValue>(TSourceValue value);
    }
}