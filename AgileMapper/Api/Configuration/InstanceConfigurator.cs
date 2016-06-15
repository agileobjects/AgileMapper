namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using Members;
    using ObjectPopulation;

    public class InstanceConfigurator<TObject> where TObject : class
    {
        private readonly MappingConfigInfo _configInfo;

        internal InstanceConfigurator(MapperContext mapperContext)
        {
            _configInfo = MappingConfigInfo.AllRuleSetsSourceTypesAndTargetTypes(mapperContext);
        }

        public void IdentifyUsing<TId>(Expression<Func<TObject, TId>> idMember)
        {
            _configInfo.MapperContext.UserConfigurations.Identifiers.Add(typeof(TObject), idMember);
        }

        public void CreateUsing(Expression<Func<ITypedMemberMappingContext<object, object>, TObject>> factory)
            => new FactorySpecifier<object, object, TObject>(_configInfo).Using(factory);

        public void CreateUsing<TFactory>(TFactory factory) where TFactory : class
            => new FactorySpecifier<object, object, TObject>(_configInfo).Using(factory);
    }
}