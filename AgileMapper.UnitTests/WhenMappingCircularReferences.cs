namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AgileMapper.Extensions;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMappingCircularReferences
    {
        [Fact]
        public void ShouldMapToANewOneToOneRelationship()
        {
            var sourceParent = new Parent { EldestChild = new Child() };
            sourceParent.EldestChild.EldestParent = sourceParent;

            var result = Mapper.Map(sourceParent).ToANew<Parent>();

            result.EldestChild.ShouldNotBeNull();
            result.EldestChild.EldestParent.ShouldBeSameAs(result);
        }

        [Fact]
        public void ShouldMapOnToAOneToOneRelationship()
        {
            var sourceParent = new Parent { EldestChild = new Child() };
            sourceParent.EldestChild.EldestParent = sourceParent;

            var targetParent = new Parent();

            var result = Mapper.Map(sourceParent).OnTo(targetParent);

            result.EldestChild.ShouldNotBeNull();
            result.EldestChild.ShouldNotBeSameAs(sourceParent.EldestChild);
            result.EldestChild.EldestParent.ShouldBeSameAs(result);
        }

        [Fact]
        public void ShouldMapOverAOneToOneRelationship()
        {
            var sourceParent = new Parent { EldestChild = new Child() };
            sourceParent.EldestChild.EldestParent = sourceParent;

            var targetParent = new Parent { EldestChild = new Child() };
            targetParent.EldestChild.EldestParent = targetParent;

            var result = Mapper.Map(sourceParent).Over(targetParent);

            result.EldestChild.ShouldNotBeNull();
            result.EldestChild.EldestParent.ShouldBeSameAs(result);
        }

        [Fact]
        public void ShouldMapToANewOneToManyRelationship()
        {
            var source = new Order
            {
                DateCreated = DateTime.Now,
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductId  = "Grass" },
                    new OrderItem { ProductId  = "Flowers" }
                }
            };

            source.Items.ForEach(item => item.Order = source);

            var result = Mapper.Clone(source);

            result.ShouldNotBeSameAs(source);
            result.Items.ShouldBe(item => item.ProductId, "Grass", "Flowers");
            result.Items.ShouldAllBe(item => item.Order == result);
        }

        [Fact]
        public void ShouldMapToANewManyToManyRelationship()
        {
            var jack = new FacebookUser { Name = "Jack" };
            var rose = new FacebookUser { Name = "Rose" };
            var brock = new FacebookUser { Name = "Brock" };

            jack.Friends = new List<FacebookUser> { rose, brock };
            rose.Friends = new List<FacebookUser> { jack, brock };
            brock.Friends = new List<FacebookUser> { jack, rose };

            var clonedJack = Mapper.Clone(jack);

            clonedJack.ShouldNotBeSameAs(jack);
            clonedJack.Name.ShouldBe("Jack");
            clonedJack.Friends.Count.ShouldBe(2);

            var clonedRose = clonedJack.Friends.First(f => f.Name == "Rose");
            clonedRose.ShouldNotBeSameAs(rose);
            clonedRose.Friends.Count.ShouldBe(2);

            var clonedBrock = clonedJack.Friends.First(f => f.Name == "Brock");
            clonedBrock.ShouldNotBeSameAs(brock);
            clonedBrock.Friends.Count.ShouldBe(2);
        }

        [Fact]
        public void ShouldGenerateAMappingPlanForAOneToOneRelationship()
        {
            var plan = Mapper
                .GetPlanFor<Parent>()
                .ToANew<Parent>();

            plan.ShouldContain("Map Parent -> Parent");
            plan.ShouldContain("Map Child -> Child");
        }
    }
}
