namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMappingOverComplexTypeMembers
    {
        [Fact]
        public void ShouldMapAMemberProperty()
        {
            var source = new Person
            {
                Address = new Address
                {
                    Line1 = "Over here!",
                    Line2 = "Yes, here!"
                }
            };

            var target = new Person
            {
                Address = new Address
                {
                    Line1 = "Over there!"
                }
            };

            var result = Mapper.Map(source).Over(target);

            result.Address.Line1.ShouldBe(source.Address.Line1);
            result.Address.Line2.ShouldBe(source.Address.Line2);
        }

        [Fact]
        public void ShouldOverwriteAMemberToNull()
        {
            var source = new Person { Name = "Dylan" };
            var target = new Person { Address = new Address { Line1 = "Over here" } };

            var result = Mapper.Map(source).Over(target);

            result.Address.ShouldBeNull();
        }

        [Fact]
        public void ShouldHandleNoMatchingSourceMember()
        {
            var source = new { Hello = "You" };
            var target = new Customer { Address = new Address() };
            var result = Mapper.Map(source).Over(target);

            result.Address.ShouldNotBeNull();
        }

        [Fact]
        public void ShouldApplyAConfiguredConstant()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.WhenMapping
                    .From<Person>()
                    .Over<Person>()
                    .Map("Big Timmy")
                    .To(x => x.Name);

                var source = new Person { Name = "Alice" };
                var target = new Person { Name = "Frank" };
                var result = mapper.Map(source).Over(target);

                result.Name.ShouldBe("Big Timmy");
            }
        }

        [Fact]
        public void ShouldApplyAConfiguredExpression()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.WhenMapping
                    .From<Customer>()
                    .Over<Person>()
                    .Map(ctx => ctx.Source.Id)
                    .To(x => x.Name);

                var source = new Customer { Id = Guid.NewGuid() };
                var target = new Person();
                var result = mapper.Map(source).Over(target);

                result.Name.ShouldBe(source.Id.ToString());
            }
        }

        [Fact]
        public void ShouldHandleANullConfiguredSourceMember()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.WhenMapping
                    .From<Person>()
                    .Over<Person>()
                    .Map(ctx => ctx.Source.Address.Line1)
                    .To(x => x.Address.Line2);

                var source = new Person { Name = "Scott" };
                var target = new Person { Address = new Address() };
                var result = mapper.Map(source).Over(target);

                result.Address.ShouldBeNull();
            }
        }
    }
}