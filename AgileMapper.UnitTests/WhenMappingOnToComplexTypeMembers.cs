namespace AgileObjects.AgileMapper.UnitTests
{
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMappingOnToComplexTypeMembers
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

            var result = Mapper.Map(source).OnTo(target);

            result.Address.Line1.ShouldNotBeNull(source.Address.Line1);
            result.Address.Line1.ShouldBe(target.Address.Line1);
            result.Address.Line2.ShouldBe(source.Address.Line2);
        }

        [Fact]
        public void ShouldHandleANullSourceMember()
        {
            var source = new Person { Name = "Freddie" };
            var target = new Person { Address = new Address { Line1 = "Over here" } };

            var result = Mapper.Map(source).OnTo(target);

            result.Address.ShouldBe(target.Address);
        }

        [Fact]
        public void ShouldApplyAConfiguredConstant()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.WhenMapping
                    .From<Person>()
                    .OnTo<Person>()
                    .Map("Hello there!")
                    .To(x => x.Address.Line2);

                var source = new Person { Name = "Alice" };
                var target = new Person { Address = new Address() };
                var result = mapper.Map(source).OnTo(target);

                result.Name.ShouldBe(source.Name);
                result.Address.ShouldNotBeNull();
                result.Address.Line2.ShouldBe("Hello there!");
            }
        }

        [Fact]
        public void ShouldApplyAConfiguredExpression()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.WhenMapping
                    .From<Person>()
                    .OnTo<Person>()
                    .Map(ctx => ctx.Source.Name + ", " + ctx.Source.Address.Line1)
                    .To(x => x.Address.Line1);

                var source = new Person { Name = "Bob", Address = new Address { Line1 = "Booooom" } };
                var target = new Person { Address = new Address() };
                var result = mapper.Map(source).OnTo(target);

                result.Name.ShouldBe(source.Name);
                result.Address.Line1.ShouldBe("Bob, Booooom");
            }
        }

        [Fact]
        public void ShouldNotOverwriteWithAConfiguredConstant()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.WhenMapping
                    .From<Person>()
                    .OnTo<Person>()
                    .Map(ctx => ctx.Source.Name + ", " + ctx.Source.Address.Line1)
                    .To(x => x.Address.Line1);

                var source = new Person { Name = "Lilly", Address = new Address { Line1 = "Booooom" } };
                var target = new Person { Address = new Address { Line1 = "Already populated!" } };
                var result = mapper.Map(source).OnTo(target);

                result.Address.Line1.ShouldBe("Already populated!");
            }
        }

        [Fact]
        public void ShouldHandleANullConfiguredSourceMember()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.WhenMapping
                    .From<Person>()
                    .OnTo<Person>()
                    .Map(ctx => ctx.Source.Address.Line1)
                    .To(x => x.Address.Line2);

                var source = new Person { Name = "Scott" };
                var target = new Person { Address = new Address() };
                var result = mapper.Map(source).OnTo(target);

                result.Address.Line2.ShouldBeNull();
            }
        }
    }
}
