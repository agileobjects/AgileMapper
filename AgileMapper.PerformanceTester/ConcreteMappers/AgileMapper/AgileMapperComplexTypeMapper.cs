namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.AgileMapper
{
    using AbstractMappers;
    using TestClasses;

    internal class AgileMapperComplexTypeMapper : ComplexTypeMapperBase
    {
        public override void Initialise()
        {
            Mapper.WhenMapping.DisableObjectTracking();
        }

        protected override Foo Clone(Foo foo)
        {
            return Mapper.Clone(foo);
        }
    }
}