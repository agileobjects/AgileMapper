namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System.Collections.Generic;
    using AgileMapper.Extensions.Internal;
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenConfiguringDerivedTypes
    {
        [Fact]
        public void ShouldMapACustomTypePair()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Product>()
                    .To<ProductDto>()
                    .Map<MegaProduct>()
                    .To<ProductDtoMega>();

                Product source = new MegaProduct { ProductId = "PrettyDarnMega", Price = 0.99, HowMega = 1.00m };

                var result = mapper.Map(source).ToANew<ProductDto>();

                result.ShouldBeOfType<ProductDtoMega>();
                result.ProductId.ShouldBe("PrettyDarnMega");
                result.Price.ShouldBe(0.99m);
                ((ProductDtoMega)result).HowMega.ShouldBe("1.00");
            }
        }

        [Fact]
        public void ShouldMapADerivedTypePairConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var exampleInstance = new { Name = default(string), Discount = default(decimal?), Report = default(string) };

                mapper.WhenMapping
                    .From(exampleInstance)
                    .ToANew<PersonViewModel>()
                    .If(s => s.Source.Discount.HasValue)
                    .MapTo<CustomerViewModel>()
                    .And
                    .If(x => !x.Source.Report.IsNullOrWhiteSpace())
                    .MapTo<MysteryCustomerViewModel>();

                var mysteryCustomerSource = new
                {
                    Name = "???",
                    Discount = (decimal?).5m,
                    Report = "Lovely!"
                };

                var mysteryCustomerResult = mapper.Map(mysteryCustomerSource).ToANew<PersonViewModel>();

                mysteryCustomerResult.ShouldBeOfType<MysteryCustomerViewModel>();
                mysteryCustomerResult.Name.ShouldBe("???");
                ((CustomerViewModel)mysteryCustomerResult).Discount.ShouldBe(0.5);
                ((MysteryCustomerViewModel)mysteryCustomerResult).Report.ShouldBe("Lovely!");

                var customerSource = new
                {
                    Name = "Firsty",
                    Discount = (decimal?)1,
                    Report = string.Empty
                };

                var customerResult = mapper.Map(customerSource).ToANew<PersonViewModel>();

                customerResult.ShouldBeOfType<CustomerViewModel>();
                customerResult.Name.ShouldBe("Firsty");
                ((CustomerViewModel)customerResult).Discount.ShouldBe(1.0);

                var personSource = new
                {
                    Name = "Datey",
                    Discount = default(decimal?),
                    Report = default(string)
                };

                var personResult = mapper.Map(personSource).ToANew<PersonViewModel>();

                personResult.ShouldBeOfType<PersonViewModel>();
                personResult.Name.ShouldBe("Datey");
            }
        }

        // See https://github.com/agileobjects/AgileMapper/issues/123
        [Fact]
        public void ShouldMapToAnInterfaceConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Issue123.GroupDto>()
                    .To<Issue123.Group>()
                    .Map(dto => dto.Eman, g => g.Name);

                mapper.WhenMapping
                    .From<Issue123.CompositeDto>().To<Issue123.IComposite>()
                    .If(ctx => ctx.Source.Type == Issue123.CompositeType.Group)
                    .MapTo<Issue123.Group>()
                    .And
                    .If(ctx => ctx.Source.Type == Issue123.CompositeType.Leaf)
                    .MapTo<Issue123.Leaf>();

                mapper.WhenMapping
                    .From<Issue123.CompositeDto>().To<Issue123.Group>()
                    .Map(ctx => ctx.Source.Group).ToTarget();

                mapper.WhenMapping
                    .From<Issue123.CompositeDto>().To<Issue123.Leaf>()
                    .Map(ctx => ctx.Source.Leaf).ToTarget();

                var groupDto = new Issue123.CompositeDto
                {
                    Type = Issue123.CompositeType.Group,
                    Group = new Issue123.GroupDto { Eman = "composite group" }
                };

                var group = mapper.Map(groupDto).ToANew<Issue123.IComposite>() as Issue123.Group;

                group.ShouldNotBeNull();

                // ReSharper disable once PossibleNullReferenceException
                group.Name.ShouldBe("composite group");
                group.Children.ShouldBeEmpty();

                var leafDto = new Issue123.CompositeDto
                {
                    Type = Issue123.CompositeType.Leaf,
                    Leaf = new Issue123.LeafDto { Description = "Leaf" }
                };

                var leaf = mapper.Map(leafDto).ToANew<Issue123.IComposite>() as Issue123.Leaf;

                leaf.ShouldNotBeNull();

                // ReSharper disable once PossibleNullReferenceException
                leaf.Description.ShouldBe("Leaf");
            }
        }

        // See https://github.com/agileobjects/AgileMapper/issues/123
        [Fact]
        public void ShouldMapToAnInterfaceInAListConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Issue123.GroupDto>()
                    .To<Issue123.Group>()
                    .Map(dto => dto.Eman, g => g.Name);

                mapper.WhenMapping
                    .From<Issue123.CompositeDto>().To<Issue123.IComposite>()
                    .If(ctx => ctx.Source.Type == Issue123.CompositeType.Group)
                    .MapTo<Issue123.Group>()
                    .And
                    .If(ctx => ctx.Source.Type == Issue123.CompositeType.Leaf)
                    .MapTo<Issue123.Leaf>();

                mapper.WhenMapping
                    .From<Issue123.CompositeDto>().To<Issue123.Group>()
                    .Map(ctx => ctx.Source.Group).ToTarget();

                mapper.WhenMapping
                    .From<Issue123.CompositeDto>().To<Issue123.Leaf>()
                    .Map(ctx => ctx.Source.Leaf).ToTarget();

                var groupDto = new Issue123.GroupDto
                {
                    Eman = "outer group",
                    Children =
                    {
                        new Issue123.CompositeDto
                        {
                            Type = Issue123.CompositeType.Group,
                            Group = new Issue123.GroupDto { Eman = "inner group" }
                        },
                        new Issue123.CompositeDto
                        {
                            Type = Issue123.CompositeType.Leaf,
                            Leaf = new Issue123.LeafDto { Description = "Leaf" }
                        }
                    }
                };

                var group = mapper.Map(groupDto).ToANew<Issue123.Group>();

                group.ShouldNotBeNull();
                group.Children.Count.ShouldBe(2);

                group.Children.First().ShouldBeOfType<Issue123.Group>();
                group.Children.Second().ShouldBeOfType<Issue123.Leaf>();
            }
        }

        [Fact]
        public void ShouldUseATypedToTarget()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Issue123.CompositeDto>().To<Issue123.IComposite>()
                    .If(ctx => ctx.Source.Type == Issue123.CompositeType.Leaf)
                    .Map(ctx => ctx.Source.Leaf)
                    .ToTarget<Issue123.Leaf>()
                    .AndWhenMapping
                    .From<Issue123.CompositeDto>().To<Issue123.Leaf>()
                    .Map((dto, l) => dto.Leaf.Description + "!")
                    .To(l => l.Description);

                var leafDto = new Issue123.CompositeDto
                {
                    Type = Issue123.CompositeType.Leaf,
                    Leaf = new Issue123.LeafDto { Description = "I am a leaf" }
                };

                var leaf = mapper.Map(leafDto).ToANew<Issue123.IComposite>() as Issue123.Leaf;

                leaf.ShouldNotBeNull();

                // ReSharper disable once PossibleNullReferenceException
                leaf.Description.ShouldBe("I am a leaf!");
            }
        }

        #region Helper Classes

        internal class Issue123
        {
            public enum CompositeType
            {
                Group,
                Leaf
            }

            // ReSharper disable MemberHidesStaticFromOuterClass
            public class CompositeDto
            {
                public CompositeType Type { get; set; }

                public GroupDto Group { get; set; }

                public LeafDto Leaf { get; set; }
            }
            // ReSharper restore MemberHidesStaticFromOuterClass

            public class GroupDto
            {
                public GroupDto()
                {
                    Children = new List<CompositeDto>();
                }

                public string Eman { get; set; }

                public IList<CompositeDto> Children { get; }
            }

            public class LeafDto
            {
                public string Description { get; set; }
            }

            public interface IComposite
            {
            }

            public interface IGroup : IComposite
            {
                string Name { get; }
            }

            public class Group : IGroup
            {
                public Group()
                {
                    Children = new List<IComposite>();
                }

                public string Name { get; set; }

                public IList<IComposite> Children { get; }
            }

            public interface ILeaf : IComposite
            {
                string Description { get; }
            }

            public class Leaf : ILeaf
            {
                public string Description { get; set; }
            }
        }

        #endregion
    }
}
