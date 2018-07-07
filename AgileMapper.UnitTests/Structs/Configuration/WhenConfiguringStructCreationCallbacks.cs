namespace AgileObjects.AgileMapper.UnitTests.Structs.Configuration
{
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenConfiguringStructCreationCallbacks
    {
        [Fact]
        public void ShouldCallAGlobalObjectCreatedCallbackWithAStruct()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var createdInstance = default(object);

                mapper.After
                    .CreatingInstances
                    .Call(ctx => createdInstance = ctx.CreatedObject);

                var source = new PublicField<long> { Value = 123456 };
                var result = mapper.Map(source).ToANew<PublicCtorStruct<int>>();

                createdInstance.ShouldNotBeNull();
                createdInstance.ShouldBeOfType<PublicCtorStruct<int>>();
                result.Value.ShouldBe(123456);
            }
        }

        [Fact]
        public void ShouldCallAnObjectCreatedCallbackForASpecifiedStructType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var createdStruct = default(PublicPropertyStruct<int>);

                mapper.After
                    .CreatingInstancesOf<PublicPropertyStruct<int>>()
                    .Call((s, t, p) => createdStruct = p);

                var source = new { Value = "12345" };
                var nonMatchingResult = mapper.Map(source).ToANew<PublicField<int>>();

                createdStruct.ShouldBeDefault();
                nonMatchingResult.Value.ShouldBe(12345);

                var matchingResult = mapper.Map(source).ToANew<PublicPropertyStruct<int>>();

                createdStruct.ShouldNotBeDefault();
                createdStruct.ShouldBe(matchingResult);
            }
        }

        [Fact]
        public void ShouldCallAnObjectCreatedCallbackForSpecifiedSourceStructType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var creationCount = 0;

                mapper.WhenMapping
                    .From<PublicPropertyStruct<string>>()
                    .To<Customer>()
                    .Map(ctx => ctx.Source.Value)
                    .To(c => c.Name)
                    .And
                    .After
                    .CreatingTargetInstances
                    .Call(ctx => ++creationCount);

                var nonMatchingSource = new { Name = "Goldblum" };
                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<Customer>();

                creationCount.ShouldBe(0);
                nonMatchingResult.Name.ShouldBe("Goldblum");

                var matchingSource = new PublicPropertyStruct<string> { Value = "Fishy" };
                var matchingResult = mapper.Map(matchingSource).ToANew<MysteryCustomer>();

                creationCount.ShouldBe(1);
                matchingResult.Name.ShouldBe("Fishy");
            }
        }

        [Fact]
        public void ShouldCallAnObjectCreatedCallbackForSpecifiedSourceAndTargetStructTypes()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var createdStruct = default(PublicCtorStruct<long>);

                mapper.WhenMapping
                    .From<PublicPropertyStruct<int>>()
                    .To<PublicCtorStruct<long>>()
                    .After
                    .CreatingTargetInstances
                    .Call(ctx => createdStruct = ctx.CreatedObject);

                var nonMatchingSource = new { Value = "8765" };
                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<PublicCtorStruct<long>>();

                createdStruct.ShouldBeDefault();
                nonMatchingResult.Value.ShouldBe(8765);

                var matchingSource = new PublicPropertyStruct<int> { Value = 5678 };
                var matchingResult = mapper.Map(matchingSource).ToANew<PublicCtorStruct<long>>();

                createdStruct.ShouldNotBeDefault();
                createdStruct.ShouldBe(matchingResult);
            }
        }
    }
}
