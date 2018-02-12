namespace AgileObjects.AgileMapper.UnitTests.Orms.Configuration
{
    using System.Threading.Tasks;
    using Infrastructure;
    using TestClasses;
    using Xunit;

    public abstract class WhenConfiguringCallbacks<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenConfiguringCallbacks(ITestContext<TOrmContext> context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldNotAttemptToCallAPreMappingCallback()
        {
            return RunTest(async context =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    var callbackCalled = false;

                    mapper.WhenMapping
                        .To<SquareViewModel>()
                        .Before
                        .MappingBegins
                        .Call(ctx => callbackCalled = true);

                    var square = new Square { SideLength = 12 };

                    await context.Shapes.Add(square);
                    await context.SaveChanges();

                    var squareVm = context
                        .Shapes
                        .ProjectUsing(mapper)
                        .To<SquareViewModel>()
                        .ShouldHaveSingleItem();

                    callbackCalled.ShouldBeFalse();
                    squareVm.SideLength.ShouldBe(12);

                    mapper.Map(square).OnTo(squareVm);

                    callbackCalled.ShouldBeTrue();
                }
            });
        }

        [Fact]
        public Task ShouldNotAttemptToCallAPostMappingCallback()
        {
            return RunTest(async context =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    var callbackCalled = false;

                    mapper.WhenMapping
                        .From<Shape>()
                        .To<CircleViewModel>()
                        .After
                        .MappingEnds
                        .Call((s, c) => callbackCalled = true);

                    var circle = new Circle { Diameter = 1 };

                    await context.Shapes.Add(circle);
                    await context.SaveChanges();

                    var circleVm = context
                        .Shapes
                        .ProjectUsing(mapper)
                        .To<CircleViewModel>()
                        .ShouldHaveSingleItem();

                    callbackCalled.ShouldBeFalse();
                    circleVm.Diameter.ShouldBe(1);

                    mapper.Map(circle).Over(circleVm);

                    callbackCalled.ShouldBeTrue();
                }
            });
        }

        [Fact]
        public Task ShouldNotAttemptToCallAPreObjectCreationCallback()
        {
            return RunTest(async context =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    var callbackCalled = false;

                    mapper.WhenMapping
                        .To<PersonDto>()
                        .Before
                        .CreatingInstancesOf<AddressDto>()
                        .Call(ctx => callbackCalled = true);

                    var person = new Person { Name = "Bjorn", Address = new Address { Line1 = "Sweden" } };

                    await context.Persons.Add(person);
                    await context.SaveChanges();

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
                }
            });
        }

        [Fact]
        public Task ShouldNotAttemptToCallAPostObjectCreationCallback()
        {
            return RunTest(async context =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    var callbackCalled = false;

                    mapper.WhenMapping
                        .To<PersonDto>()
                        .After
                        .CreatingInstances
                        .Call(ctx => callbackCalled = true);

                    var person = new Person { Name = "Benny", Address = new Address { Line1 = "Sweden" } };

                    await context.Persons.Add(person);
                    await context.SaveChanges();

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
                }
            });
        }

        [Fact]
        public Task ShouldNotAttemptToCallAPreMemberMappingCallback()
        {
            return RunTest(async context =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    var callbackCalled = false;

                    mapper.WhenMapping
                        .To<SquareViewModel>()
                        .Before
                        .Mapping(s => s.SideLength)
                        .Call(ctx => callbackCalled = true);

                    var square = new Square { SideLength = 1 };

                    await context.Shapes.Add(square);
                    await context.SaveChanges();

                    var squareVm = context
                        .Shapes
                        .ProjectUsing(mapper)
                        .To<SquareViewModel>()
                        .ShouldHaveSingleItem();

                    callbackCalled.ShouldBeFalse();
                    squareVm.SideLength.ShouldBe(1);

                    mapper.Map(square).Over(squareVm);

                    callbackCalled.ShouldBeTrue();
                }
            });
        }

        [Fact]
        public Task ShouldNotAttemptToCallAPostMemberMappingCallback()
        {
            return RunTest(async context =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    var callbackCalled = false;

                    mapper.WhenMapping
                        .To<CircleViewModel>()
                        .After
                        .Mapping(c => c.Diameter)
                        .Call(ctx => callbackCalled = true);

                    var circle = new Circle { Diameter = 11 };

                    await context.Shapes.Add(circle);
                    await context.SaveChanges();

                    var circleVm = context
                        .Shapes
                        .ProjectUsing(mapper)
                        .To<CircleViewModel>()
                        .ShouldHaveSingleItem();

                    callbackCalled.ShouldBeFalse();
                    circleVm.Diameter.ShouldBe(11);

                    mapper.Map(circle).Over(circleVm);

                    callbackCalled.ShouldBeTrue();
                }
            });
        }
    }
}
