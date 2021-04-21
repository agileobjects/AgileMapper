namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using AgileMapper.Configuration.DataSources;

    internal interface ISequencedDataSourceFactory
    {
        void SetTargetCtorParameter(ParameterInfo parameter);

        void SetTargetMember(LambdaExpression targetMember);

        void Register(
            Func<IConfiguredDataSourceFactoryFactory, ConfiguredDataSourceFactory> dataSourceFactoryFactory,
            Type targetMemberType);
    }
}