namespace AgileObjects.AgileMapper.Buildable.UnitTests.MapperConfiguration
{
    using AgileMapper.UnitTests.Common.TestClasses;
    using Buildable.Configuration;

    public class DerivedTypeMapperConfiguration : BuildableMapperConfiguration
    {
        protected override void Configure()
        {
            GetPlanFor<Product>().ToANew<ProductDto>(cfg => cfg
                .Map<MegaProduct>().To<ProductDtoMega>());
        }
    }
}