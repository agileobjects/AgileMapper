namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.Mapster
{
    using AbstractMappers;
    using global::Mapster;
    using TestClasses;

    internal class MapsterDeepMapper : DeepMapperBase
    {
        public override void Initialise()
        {
        }

        protected override CustomerDto Map(Customer customer)
        {
            return customer.Adapt<CustomerDto>();
        }
    }
}