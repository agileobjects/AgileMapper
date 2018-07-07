namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Globalization;
    using System.Linq.Expressions;
    using AgileMapper.Configuration;
#if NET35
    using Extensions.Internal;
#endif
    using Members;
    using ObjectPopulation;
    using Projection;
    using ReadableExpressions.Extensions;

    internal class FactorySpecifier<TSource, TTarget, TObject> :
        IMappingFactorySpecifier<TSource, TTarget, TObject>,
        IProjectionFactorySpecifier<TSource, TTarget, TObject>
    {
        private readonly MappingConfigInfo _configInfo;

        public FactorySpecifier(MappingConfigInfo configInfo)
        {
            _configInfo = configInfo;
        }

        public IMappingConfigContinuation<TSource, TTarget> Using(
            Expression<Func<IMappingData<TSource, TTarget>, TObject>> factory)
#if NET35
            => RegisterObjectFactory(factory.ToDlrExpression(), ConfiguredObjectFactory.For);
#else
            => RegisterObjectFactory(factory, ConfiguredObjectFactory.For);
#endif
        public IProjectionConfigContinuation<TSource, TTarget> Using(Expression<Func<TSource, TObject>> factory)
#if NET35
            => RegisterObjectFactory(factory.ToDlrExpression(), ConfiguredObjectFactory.For);
#else
            => RegisterObjectFactory(factory, ConfiguredObjectFactory.For);
#endif
        public IMappingConfigContinuation<TSource, TTarget> Using(LambdaExpression factory)
#if NET35
            => RegisterObjectFactory(factory.ToDlrExpression(), ConfiguredObjectFactory.For);
#else
            => RegisterObjectFactory(factory, ConfiguredObjectFactory.For);
#endif
        public IMappingConfigContinuation<TSource, TTarget> Using<TFactory>(TFactory factory)
            where TFactory : class
        {
            var factoryInfo = ConfiguredLambdaInfo.ForFunc(factory, typeof(TSource), typeof(TTarget));

            if (factoryInfo != null)
            {
                return RegisterObjectFactory(factoryInfo, ConfiguredObjectFactory.For);
            }

            var contextTypeName = typeof(IMappingData<TSource, TTarget>).GetFriendlyName();
            var sourceTypeName = typeof(TSource).GetFriendlyName();
            var targetTypeName = typeof(TTarget).GetFriendlyName();
            var objectTypeName = typeof(TObject).GetFriendlyName();

            string[] validSignatures =
            {
                $"Func<{objectTypeName}>",
                $"Func<{contextTypeName}, {objectTypeName}>",
                $"Func<{sourceTypeName}, {targetTypeName}, {objectTypeName}>",
                $"Func<{sourceTypeName}, {targetTypeName}, int?, {objectTypeName}>"
            };

            throw new MappingConfigurationException(string.Format(
                CultureInfo.InvariantCulture,
                "Unable to create objects of type {0} using factory {1}: valid function signatures are {2}",
                objectTypeName,
                typeof(TFactory).GetFriendlyName(),
                string.Join(", ", validSignatures)));
        }

        private MappingConfigContinuation<TSource, TTarget> RegisterObjectFactory<TFactory>(
            TFactory factory,
            Func<MappingConfigInfo, Type, TFactory, ConfiguredObjectFactory> objectFactoryFactory)
        {
            var objectFactory = objectFactoryFactory.Invoke(_configInfo, typeof(TObject), factory);

            _configInfo.MapperContext.UserConfigurations.Add(objectFactory);

            return new MappingConfigContinuation<TSource, TTarget>(_configInfo);
        }
    }
}