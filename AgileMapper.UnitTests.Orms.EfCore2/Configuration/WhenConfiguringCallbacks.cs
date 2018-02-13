namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Configuration
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Infrastructure;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringCallbacks : OrmTestClassBase<EfCore2TestDbContext>
    {
        public WhenConfiguringCallbacks(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldNotAttemptToCallAPreMappingCallback()
        {
            return RunTest(async (context, mapper) =>
            {
                var callbackCalled = false;

                mapper.WhenMapping
                    .To<SquareViewModel>()
                    .Before
                    .MappingBegins
                    .Call(ctx => callbackCalled = true);

                var square = new Square { SideLength = 12 };

                await context.Shapes.AddAsync(square);
                await context.SaveChangesAsync();

                var squareVm = context
                    .Shapes
                    .ProjectUsing(mapper)
                    .To<SquareViewModel>()
                    .ShouldHaveSingleItem();

                callbackCalled.ShouldBeFalse();
                squareVm.SideLength.ShouldBe(12);

                mapper.Map(square).OnTo(squareVm);

                callbackCalled.ShouldBeTrue();
            });
        }

        [Fact]
        public Task ShouldNotAttemptToCallAPostMappingCallback()
        {
            return RunTest(async (context, mapper) =>
            {
                var callbackCalled = false;

                mapper.WhenMapping
                    .From<Shape>()
                    .To<CircleViewModel>()
                    .After
                    .MappingEnds
                    .Call((s, c) => callbackCalled = true);

                var circle = new Circle { Diameter = 1 };

                await context.Shapes.AddAsync(circle);
                await context.SaveChangesAsync();

                var circleVm = context
                    .Shapes
                    .ProjectUsing(mapper)
                    .To<CircleViewModel>()
                    .ShouldHaveSingleItem();

                callbackCalled.ShouldBeFalse();
                circleVm.Diameter.ShouldBe(1);

                mapper.Map(circle).Over(circleVm);

                callbackCalled.ShouldBeTrue();
            });
        }

        [Fact]
        public Task ShouldNotAttemptToCallAPreObjectCreationCallback()
        {
            return RunTest(async (context, mapper) =>
            {
                var callbackCalled = false;

                mapper.WhenMapping
                    .To<PersonDto>()
                    .Before
                    .CreatingInstancesOf<AddressDto>()
                    .Call(ctx => callbackCalled = true);

                var person = new Person { Name = "Bjorn", Address = new Address { Line1 = "Sweden" } };

                await context.Persons.AddAsync(person);
                await context.SaveChangesAsync();

                var personDto = context
                    .Persons
                    .ProjectUsing(mapper)
                    .To<PersonDto>()
                    .ShouldHaveSingleItem();

                callbackCalled.ShouldBeFalse();
                personDto.Name.ShouldBe("Bjorn");
                personDto.Address.ShouldNotBeNull();
                personDto.Address.Line1.ShouldBe("Sweden");

                mapper.Map(person).Over(personDto);

                callbackCalled.ShouldBeTrue();
            });
        }

        [Fact]
        public Task ShouldNotAttemptToCallAPostObjectCreationCallback()
        {
            return RunTest(async (context, mapper) =>
            {
                var callbackCalled = false;

                mapper.WhenMapping
                    .To<PersonDto>()
                    .After
                    .CreatingInstances
                    .Call(ctx => callbackCalled = true);

                var person = new Person { Name = "Benny", Address = new Address { Line1 = "Sweden" } };

                await context.Persons.AddAsync(person);
                await context.SaveChangesAsync();

                var personDto = context
                    .Persons
                    .ProjectUsing(mapper)
                    .To<PersonDto>()
                    .ShouldHaveSingleItem();

                callbackCalled.ShouldBeFalse();
                personDto.Name.ShouldBe("Benny");
                personDto.Address.ShouldNotBeNull();
                personDto.Address.Line1.ShouldBe("Sweden");

                mapper.Map(person).ToANew<PersonDto>();

                callbackCalled.ShouldBeTrue();
            });
        }

        [Fact]
        public Task ShouldNotAttemptToCallAPreMemberMappingCallback()
        {
            return RunTest(async (context, mapper) =>
            {
                var callbackCalled = false;

                mapper.WhenMapping
                    .To<SquareViewModel>()
                    .Before
                    .Mapping(s => s.SideLength)
                    .Call(ctx => callbackCalled = true);

                var square = new Square { SideLength = 1 };

                await context.Shapes.AddAsync(square);
                await context.SaveChangesAsync();

                var squareVm = context
                    .Shapes
                    .ProjectUsing(mapper)
                    .To<SquareViewModel>()
                    .ShouldHaveSingleItem();

                callbackCalled.ShouldBeFalse();
                squareVm.SideLength.ShouldBe(1);

                mapper.Map(square).Over(squareVm);

                callbackCalled.ShouldBeTrue();
            });
        }

        [Fact]
        public Task ShouldNotAttemptToCallAPostMemberMappingCallback()
        {
            return RunTest(async (context, mapper) =>
            {
                var callbackCalled = false;

                mapper.WhenMapping
                    .To<CircleViewModel>()
                    .After
                    .Mapping(c => c.Diameter)
                    .Call(ctx => callbackCalled = true);

                var circle = new Circle { Diameter = 11 };

                await context.Shapes.AddAsync(circle);
                await context.SaveChangesAsync();

                var circleVm = context
                    .Shapes
                    .ProjectUsing(mapper)
                    .To<CircleViewModel>()
                    .ShouldHaveSingleItem();

                callbackCalled.ShouldBeFalse();
                circleVm.Diameter.ShouldBe(11);

                mapper.Map(circle).Over(circleVm);

                callbackCalled.ShouldBeTrue();
            });
        }
    }
}
