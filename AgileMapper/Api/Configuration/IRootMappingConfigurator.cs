namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using Members;

    public interface IRootMappingConfigurator<TSource, TTarget>
    {
        void CreateInstancesUsing(Expression<Func<ITypedMemberMappingContext<TSource, TTarget>, TTarget>> factory);

        void CreateInstancesUsing<TFactory>(TFactory factory) where TFactory : class;

        IFactorySpecifier<TSource, TTarget, TObject> CreateInstancesOf<TObject>() where TObject : class;

        void Ignore(params Expression<Func<TTarget, object>>[] targetMembers);

        CustomDataSourceTargetMemberSpecifier<TTarget> Map<TSourceValue>(
            Expression<Func<ITypedMemberMappingContext<TSource, TTarget>, TSourceValue>> valueFactoryExpression);

        CustomDataSourceTargetMemberSpecifier<TTarget> Map<TSourceValue>(
            Expression<Func<TSource, TTarget, TSourceValue>> valueFactoryExpression);

        CustomDataSourceTargetMemberSpecifier<TTarget> Map<TSourceValue>(
            Expression<Func<TSource, TTarget, int?, TSourceValue>> valueFactoryExpression);

        CustomDataSourceTargetMemberSpecifier<TTarget> MapFunc<TSourceValue>(
            Func<TSource, TSourceValue> valueFunc);

        CustomDataSourceTargetMemberSpecifier<TTarget> Map<TSourceValue>(TSourceValue value);
    }
}