namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;

    public class InstanceConfigurator<TInstance>
    {
        private readonly MapperContext _mapperContext;

        internal InstanceConfigurator(MapperContext mapperContext)
        {
            _mapperContext = mapperContext;
        }

        public void IdentifyUsing<TId>(Expression<Func<TInstance, TId>> idMember)
        {
            _mapperContext.UserConfigurations.Identifiers.Add(typeof(TInstance), idMember);
        }

        public void CreateUsing(Expression<Func<TInstance>> objectFactory)
        {
            _mapperContext.UserConfigurations.ObjectFactories.Add(typeof(TInstance), objectFactory);
        }
    }
}