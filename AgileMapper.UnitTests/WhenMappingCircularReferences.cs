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
        public void ShouldMapToANewOneToManyViaIntermediateRelationship()
        {
            var pilot = new Pilot
            {
                Name = "Walls",
                Qualifications = new PilotQualifications
                {
                    TrainedAeroplanes = new List<Aeroplane>()
                }
            };

            var concorde = new Aeroplane { Model = "Concorde", Pilot = pilot };
            var f16 = new Aeroplane { Model = "F16", Pilot = pilot };

            pilot.Qualifications.TrainedAeroplanes.Add(concorde);
            pilot.Qualifications.TrainedAeroplanes.Add(f16);

            var clonedPilot = Mapper.Clone(pilot);

            clonedPilot.ShouldNotBeSameAs(pilot);
            clonedPilot.Name.ShouldBe("Walls");
            clonedPilot.Qualifications.ShouldNotBeNull();
            clonedPilot.Qualifications.ShouldNotBeSameAs(pilot.Qualifications);
            clonedPilot.Qualifications.TrainedAeroplanes.ShouldNotBeNull();
            clonedPilot.Qualifications.TrainedAeroplanes.Count.ShouldBe(2);

            clonedPilot.Qualifications.TrainedAeroplanes.First().ShouldNotBeSameAs(concorde);
            clonedPilot.Qualifications.TrainedAeroplanes.First().Model.ShouldBe("Concorde");
            clonedPilot.Qualifications.TrainedAeroplanes.First().Pilot.ShouldBe(clonedPilot);

            clonedPilot.Qualifications.TrainedAeroplanes.Second().ShouldNotBeSameAs(f16);
            clonedPilot.Qualifications.TrainedAeroplanes.Second().Model.ShouldBe("F16");
            clonedPilot.Qualifications.TrainedAeroplanes.Second().Pilot.ShouldBe(clonedPilot);
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
        public void ShouldMapMultiplyRecursiveRelationships()
        {
            var recursorOne = new MultipleRecursor { Name = "One" };
            var recursorTwo = new MultipleRecursor { Name = "Two" };

            var recursorThree = new MultipleRecursor
            {
                Name = "Three",
                ChildRecursor = new MultipleRecursor
                {
                    Name = "Three.ChildRecursor",
                    ChildRecursorArray = new[] { recursorTwo }
                }
            };

            var source = new MultipleRecursor
            {
                Name = "Root",
                ChildRecursor = new MultipleRecursor
                {
                    Name = "Root.ChildRecursor",
                    ChildRecursor = recursorOne
                },
                ChildRecursorArray = new[]
                {
                    new MultipleRecursor { Name = "Root.ChildRecursorArray[0]" },
                    recursorOne,
                    new MultipleRecursor
                    {
                        Name = "Root.ChildRecursorArray[2]",
                        ChildRecursor = recursorThree
                    }
                },
                ChildRecursors = new List<MultipleRecursor>
                {
                    recursorTwo,
                    new MultipleRecursor { Name = "Root.ChildRecursors[1]" },
                    recursorThree
                }
            };

            var result = Mapper.Map(source).ToANew<MultipleRecursor>();

            result.ShouldNotBeNull();
            result.ShouldNotBeSameAs(source);

            result.Name.ShouldBe("Root");

            result.ChildRecursor.ShouldNotBeNull();
            result.ChildRecursor.Name.ShouldBe("Root.ChildRecursor");

            var clonedRecursorOne = result.ChildRecursor.ChildRecursor;
            clonedRecursorOne.ShouldNotBeNull();
            clonedRecursorOne.ShouldNotBeSameAs(recursorOne);
            clonedRecursorOne.Name.ShouldBe("One");

            result.ChildRecursorArray.ShouldNotBeNull();
            result.ChildRecursorArray.Length.ShouldBe(3);
            result.ChildRecursorArray.First().Name.ShouldBe("Root.ChildRecursorArray[0]");
            result.ChildRecursorArray.Second().ShouldBeSameAs(clonedRecursorOne);
            result.ChildRecursorArray.Third().Name.ShouldBe("Root.ChildRecursorArray[2]");

            var clonedRecursorThree = result.ChildRecursorArray.Third().ChildRecursor;
            clonedRecursorThree.ShouldNotBeNull();
            clonedRecursorThree.ShouldNotBeSameAs(recursorThree);
            clonedRecursorThree.Name.ShouldBe("Three");
            clonedRecursorThree.ChildRecursor.ShouldNotBeNull();
            clonedRecursorThree.ChildRecursor.Name.ShouldBe("Three.ChildRecursor");
            clonedRecursorThree.ChildRecursor.ChildRecursorArray.ShouldNotBeNull();
            clonedRecursorThree.ChildRecursor.ChildRecursorArray.Length.ShouldBe(1);

            var clonedRecursorTwo = clonedRecursorThree.ChildRecursor.ChildRecursorArray.First();
            clonedRecursorTwo.ShouldNotBeNull();
            clonedRecursorTwo.ShouldNotBeSameAs(recursorTwo);
            clonedRecursorTwo.Name.ShouldBe("Two");

            result.ChildRecursors.ShouldNotBeNull();
            result.ChildRecursors.Count.ShouldBe(3);
            result.ChildRecursors.First().ShouldBeSameAs(clonedRecursorTwo);
            result.ChildRecursors.Second().Name.ShouldBe("Root.ChildRecursors[1]");
            result.ChildRecursors.Third().ShouldBeSameAs(clonedRecursorThree);
        }

        [Fact]
        public void ShouldGenerateAMappingPlanForAOneToOneRelationship()
        {
            var plan = Mapper
                .GetPlanFor<Parent>()
                .ToANew<Parent>();

            plan.ShouldContain("Map Parent -> Parent");
            plan.ShouldContain("mapEldestChildToChild"); // The name of the recusive mapper function
        }

        #region Helper Classes

        internal class Aeroplane
        {
            public string Model { get; set; }

            public Pilot Pilot { get; set; }
        }

        internal class Pilot
        {
            public string Name { get; set; }

            public PilotQualifications Qualifications { get; set; }
        }

        internal class PilotQualifications
        {
            public IList<Aeroplane> TrainedAeroplanes { get; set; }
        }

        internal class MultipleRecursor
        {
            public string Name { get; set; }

            public MultipleRecursor ChildRecursor { get; set; }

            public MultipleRecursor[] ChildRecursorArray { get; set; }

            public List<MultipleRecursor> ChildRecursors { get; set; }
        }

        #endregion
    }
}
