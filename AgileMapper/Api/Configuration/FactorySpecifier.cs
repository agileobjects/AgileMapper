namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using Members;
    using ObjectPopulation;

    internal class FactorySpecifier<TSource, TTarget, TObject> : IFactorySpecifier<TSource, TTarget, TObject>
    {
        private readonly MappingConfigInfo _configInfo;

        public FactorySpecifier(MappingConfigInfo configInfo)
        {
            _configInfo = configInfo.ForTargetType(typeof(TTarget));
        }

        public void Using(Expression<Func<ITypedMemberMappingContext<TSource, TTarget>, TObject>> factory)
        {
            var objectFactory = ConfiguredObjectFactory.For(_configInfo, typeof(TObject), factory);

            _configInfo.MapperContext.UserConfigurations.Add(objectFactory);
        }

        public void Using<TFactory>(TFactory factory) where TFactory : class
        {
            var factoryInfo = ConfiguredLambdaInfo.ForFunc(factory, typeof(TSource), typeof(TTarget));
            var objectFactory = ConfiguredObjectFactory.For(_configInfo, typeof(TObject), factoryInfo);

            _configInfo.MapperContext.UserConfigurations.Add(objectFactory);
        }
    }
}