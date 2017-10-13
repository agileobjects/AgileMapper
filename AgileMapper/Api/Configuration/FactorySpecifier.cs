namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Globalization;
    using System.Linq.Expressions;
    using AgileMapper.Configuration;
    using Members;
    using ObjectPopulation;
    using ReadableExpressions.Extensions;

    internal class FactorySpecifier<TSource, TTarget, TObject> : IFactorySpecifier<TSource, TTarget, TObject>
    {
        private readonly MappingConfigInfo _configInfo;

        public FactorySpecifier(MappingConfigInfo configInfo)
        {
            _configInfo = configInfo;
        }

        public void Using(Expression<Func<IMappingData<TSource, TTarget>, TObject>> factory)
        {
            var objectFactory = ConfiguredObjectFactory.For(_configInfo, typeof(TObject), factory);

            _configInfo.MapperContext.UserConfigurations.Add(objectFactory);
        }

        public void Using<TFactory>(TFactory factory) where TFactory : class
        {
            var factoryInfo = ConfiguredLambdaInfo.ForFunc(factory, typeof(TSource), typeof(TTarget));

            if (factoryInfo == null)
            {
                var contextTypeName = typeof(IMappingData<TSource, TTarget>).GetFriendlyName();
                var sourceTypeName = typeof(TSource).GetFriendlyName();
                var targetTypeName = typeof(TTarget).GetFriendlyName();
                var objectTypeName = typeof(TObject).GetFriendlyName();

                string[] validSignatures =
                {
                    "Func<" + objectTypeName + ">",
                    $"Func<{contextTypeName}, {objectTypeName}>",
                    $"Func<{sourceTypeName}, {targetTypeName}, {objectTypeName}>",
                    $"Func<{sourceTypeName}, {targetTypeName}, int?, {objectTypeName}>"
                };

                throw new MappingConfigurationException(string.Format(
                    CultureInfo.InvariantCulture,
                    "Unable to create objects of type {0} using factory {1}: valid function signatures are {2}",
                    objectTypeName,
                    factory,
                    string.Join(", ", validSignatures)));
            }

            var objectFactory = ConfiguredObjectFactory.For(_configInfo, typeof(TObject), factoryInfo);

            _configInfo.MapperContext.UserConfigurations.Add(objectFactory);
        }
    }
}