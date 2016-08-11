namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using Members;

    public interface IFactorySpecifier<TSource, TTarget, TObject>
    {
        void Using(Expression<Func<IMappingData<TSource, TTarget>, TObject>> factory);

        void Using<TFactory>(TFactory factory) where TFactory : class;
    }
}