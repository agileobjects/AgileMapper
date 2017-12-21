namespace AgileObjects.AgileMapper.UnitTests
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using TestClasses;
    using Xunit;

    public class WhenMappingToNewEnumerables
    {
        [Fact]
        public void ShouldCreateASimpleTypeArray()
        {
            var source = new[] { 1, 2, 3 };
            var result = Mapper.Map(source).ToANew<int[]>();

            result.ShouldNotBeNull();
            result.ShouldNotBeSameAs(source);
            result.SequenceEqual(source).ShouldBeTrue();
        }

        [Fact]
        public void ShouldCreateASimpleTypeList()
        {
            var source = new[] { 'O', 'M', 'G' };
            var result = Mapper.Map(source).ToANew<List<char>>();

            result.ShouldNotBeNull();
            result.SequenceEqual(source).ShouldBeTrue();
        }

        [Fact]
        public void ShouldCreateAConvertedSimpleTypeCollection()
        {
            var source = new List<string> { "1", "2", "3" };
            var result = Mapper.Map(source).ToANew<Collection<byte?>>();

            result.ShouldNotBeNull();
            result.ShouldBe<byte?>(1, 2, 3);
        }

        [Fact]
        public void ShouldCreateASimpleTypeEnumerable()
        {
            var source = new List<string> { "One", "Two", "Three" };
            var result = Mapper.Map(source).ToANew<IEnumerable<string>>();

            result.ShouldNotBeNull();
            result.ShouldNotBeSameAs(source);
            result.SequenceEqual(source).ShouldBeTrue();
        }

        [Fact]
        public void ShouldCreateAComplexTypeList()
        {
            var source = new List<Person>
            {
                new Person { Name = "Pete", Address = new Address() },
                new Person { Name = "Johnny", Address = new Address() }
            };

            var result = Mapper.Map(source).ToANew<List<Person>>();

            result.ShouldNotBeNull();
            result.ShouldNotBe(source);
            result.First().ShouldNotBe(source.First());
            result.First().Name.ShouldBe("Pete");
            result.Second().ShouldNotBe(source.Second());
            result.Second().Name.ShouldBe("Johnny");
        }

        [Fact]
        public void ShouldCreateAComplexTypeArrayUsingRuntimeTypedElements()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping.MaintainIdentityIntegrity();

                var viewModel = new CustomerViewModel { Name = "Dave", AddressLine1 = "View model!" };
                var source = new List<PersonViewModel> { viewModel, viewModel };
                var result = mapper.Map(source).ToANew<Person[]>();

                result.First().ShouldBeOfType<Customer>();
                result.First().Name.ShouldBe("Dave");
                result.First().Address.ShouldNotBeNull();
                result.First().Address.Line1.ShouldBe("View model!");

                result.Second().ShouldBeSameAs(result.First());
            }
        }

        [Fact]
        public void ShouldCreateAReadOnlyCollection()
        {
            var source = new[] { 1, 2, 3 };
            var result = Mapper.Map(source).ToANew<ReadOnlyCollection<int>>();

            result.ShouldNotBeNull();
            result.ShouldBe(1, 2, 3);
        }

        [Fact]
        public void ShouldMapFromAReadOnlyCollection()
        {
            var source = new ReadOnlyCollection<long>(new[] { 1, 2, 3L });
            var result = Mapper.Map(source).ToANew<int[]>();

            result.ShouldNotBeNull();
            result.ShouldBe(1, 2, 3);
        }

        [Fact]
        public void ShouldCreateAnIReadOnlyCollection()
        {
            var source = new[] { 1, 2, 3 };
            var result = Mapper.Map(source).ToANew<IReadOnlyCollection<short>>();

            result.ShouldNotBeNull();
            result.ShouldBe((short)1, (short)2, (short)3);
        }

        [Fact]
        public void ShouldHandleANullComplexTypeElement()
        {
            var source = new List<Product>
            {
                new Product { ProductId = "Surprise" },
                null,
                new Product { ProductId = "Boomstick" }
            };

            var result = Mapper.Map(source).ToANew<List<Product>>();

            result.ShouldNotBeNull();
            result.ShouldNotBe(source);
            result.Second().ShouldBeNull();
        }

        [Fact]
        public void ShouldCreateAnEmptyListByDefault()
        {
            var source = new PublicProperty<string>();
            var result = Mapper.Map(source).ToANew<List<Person>>();

            result.ShouldNotBeNull();
            result.ShouldBeEmpty();
        }
    }
}
