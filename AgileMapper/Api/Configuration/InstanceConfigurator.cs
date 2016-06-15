namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using ObjectPopulation;

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

        public void CreateUsing(Expression<Func<TInstance>> factory)
        {
            var objectFactory = ConfiguredObjectFactory.For(_mapperContext, typeof(TInstance), factory);

            _mapperContext.UserConfigurations.Add(objectFactory);
        }

        public void CreateUsing<TFactory>(TFactory factory) where TFactory : class
        {
            var valueLambdaInfo = ConfiguredLambdaInfo.ForFunc(factory);
            var objectFactory = ConfiguredObjectFactory.For(_mapperContext, typeof(TInstance), valueLambdaInfo);

            _mapperContext.UserConfigurations.Add(objectFactory);
        }
    }
}