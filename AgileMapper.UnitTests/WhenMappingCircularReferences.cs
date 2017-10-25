namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using AgileMapper.Extensions;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMappingCircularReferences
    {
        [Fact]
        public void ShouldMapToANewOneToOneRelationship()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var sourceParent = new Parent { EldestChild = new Child() };
                sourceParent.EldestChild.EldestParent = sourceParent;

                var result = mapper.Map(sourceParent).ToANew<Parent>();

                result.EldestChild.ShouldNotBeNull();
                result.EldestChild.EldestParent.ShouldBeSameAs(result);
            }
        }

        [Fact]
        public void ShouldMapOnToAOneToOneRelationship()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var sourceParent = new Parent { EldestChild = new Child() };
                sourceParent.EldestChild.EldestParent = sourceParent;

                var targetParent = new Parent();

                var result = mapper.Map(sourceParent).OnTo(targetParent);

                result.EldestChild.ShouldNotBeNull();
                result.EldestChild.ShouldNotBeSameAs(sourceParent.EldestChild);
                result.EldestChild.EldestParent.ShouldBeSameAs(result);
            }
        }

        [Fact]
        public void ShouldMapOverAOneToOneRelationship()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var sourceParent = new Parent { EldestChild = new Child() };
                sourceParent.EldestChild.EldestParent = sourceParent;

                var targetParent = new Parent { EldestChild = new Child() };
                targetParent.EldestChild.EldestParent = targetParent;

                var result = mapper.Map(sourceParent).Over(targetParent);

                result.EldestChild.ShouldNotBeNull();
                result.EldestChild.EldestParent.ShouldBeSameAs(result);
            }
        }

        [Fact]
        public void ShouldMapToAOneToOneRelationshipWithGlobalObjectTrackingDisabled()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping.DisableObjectTracking();

                var sourceParent = new Parent { EldestChild = new Child() };

                var result = mapper.Map(sourceParent).ToANew<Parent>();

                result.EldestChild.ShouldNotBeNull();
                result.EldestChild.EldestParent.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldMapToANewOneToManyViaIntermediateRelationship()
        {
            using (var mapper = Mapper.CreateNew())
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

                var clonedPilot = mapper.Clone(pilot);

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
        }

        [Fact]
        public void ShouldMapToANewOneToManyViaIntermediateRelationshipWithObjectTrackingDisabled()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .ToANew<Aeroplane>()
                    .DisableObjectTracking();

                var pilot = new Pilot
                {
                    Name = "Walls",
                    Qualifications = new PilotQualifications
                    {
                        TrainedAeroplanes = new List<Aeroplane>
                        {
                            new Aeroplane { Model = "Concorde" },
                            new Aeroplane { Model = "F16" }
                        }
                    }
                };

                var clonedPilot = mapper.Clone(pilot);

                clonedPilot.ShouldNotBeSameAs(pilot);
                clonedPilot.Name.ShouldBe("Walls");
                clonedPilot.Qualifications.ShouldNotBeNull();
                clonedPilot.Qualifications.ShouldNotBeSameAs(pilot.Qualifications);
                clonedPilot.Qualifications.TrainedAeroplanes.ShouldNotBeNull();
                clonedPilot.Qualifications.TrainedAeroplanes.Count.ShouldBe(2);

                var concorde = pilot.Qualifications.TrainedAeroplanes.First();
                clonedPilot.Qualifications.TrainedAeroplanes.First().ShouldNotBeSameAs(concorde);
                clonedPilot.Qualifications.TrainedAeroplanes.First().Model.ShouldBe("Concorde");
                clonedPilot.Qualifications.TrainedAeroplanes.First().Pilot.ShouldBeNull();

                var f16 = pilot.Qualifications.TrainedAeroplanes.Second();
                clonedPilot.Qualifications.TrainedAeroplanes.Second().ShouldNotBeSameAs(f16);
                clonedPilot.Qualifications.TrainedAeroplanes.Second().Model.ShouldBe("F16");
                clonedPilot.Qualifications.TrainedAeroplanes.Second().Pilot.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldMapToANewOneToManyRelationship()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var source = new Order
                {
                    DateCreated = DateTime.Now,
                    Items = new List<OrderItem>
                    {
                        new OrderItem {ProductId = "Grass"},
                        new OrderItem {ProductId = "Flowers"}
                    }
                };

                source.Items.ForEach(item => item.Order = source);

                var result = mapper.Clone(source);

                result.ShouldNotBeSameAs(source);
                result.Items.ShouldBe(item => item.ProductId, "Grass", "Flowers");
                result.Items.ShouldAllBe(item => item.Order == result);
            }
        }

        [Fact]
        public void ShouldMapToANewManyToManyRelationship()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var jack = new FacebookUser { Name = "Jack" };
                var rose = new FacebookUser { Name = "Rose" };
                var brock = new FacebookUser { Name = "Brock" };

                jack.Friends = new List<FacebookUser> { rose, brock };
                rose.Friends = new List<FacebookUser> { jack, brock };
                brock.Friends = new List<FacebookUser> { jack, rose };

                var clonedJack = mapper.Clone(jack);

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
        }

        [Fact]
        public void ShouldMapMultiplyRecursiveRelationships()
        {
            using (var mapper = Mapper.CreateNew())
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
                        new MultipleRecursor {Name = "Root.ChildRecursorArray[0]"},
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
                        new MultipleRecursor {Name = "Root.ChildRecursors[1]"},
                        recursorThree
                    }
                };

                var result = mapper.Map(source).ToANew<MultipleRecursor>();

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
        }

        [Fact]
        public void ShouldMapNestedMultiplyRecursiveRelationships()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var recursorOne = new MultipleRecursor { Name = "One" };
                var recursorTwo = new MultipleRecursor { Name = "Two" };

                recursorOne.ChildRecursor = recursorTwo;
                recursorTwo.ChildRecursor = recursorOne;

                var source = new PublicField<MultipleRecursor[]>
                {
                    Value = new[] { recursorOne, recursorTwo }
                };

                var result = mapper.Map(source).ToANew<PublicField<ReadOnlyCollection<MultipleRecursor>>>();

                result.ShouldNotBeNull();
                result.ShouldNotBeSameAs(source);
                result.Value.Count.ShouldBe(2);

                var resultOne = result.Value.First();
                resultOne.ShouldNotBeSameAs(recursorOne);
                resultOne.Name.ShouldBe("One");
                resultOne.ChildRecursor.ShouldNotBeNull();
                resultOne.ChildRecursor.ShouldNotBeSameAs(recursorTwo);

                var resultTwo = result.Value.Second();
                resultTwo.ShouldNotBeSameAs(recursorTwo);
                resultTwo.Name.ShouldBe("Two");
                resultTwo.ChildRecursor.ShouldNotBeNull();
                resultTwo.ChildRecursor.ShouldNotBeSameAs(recursorOne);

                resultOne.ChildRecursor.ShouldBeSameAs(resultTwo);
                resultTwo.ChildRecursor.ShouldBeSameAs(resultOne);
            }
        }

        [Fact]
        public void ShouldGenerateAMappingPlanForLinkRelationships()
        {
            string plan = Mapper.GetPlanFor<Video>().Over<Video>();

            plan.ShouldNotBeNull();
            plan.ShouldContain("WhenMappingCircularReferences.Video -> WhenMappingCircularReferences.Video");
            plan.ShouldContain(".MapRecursion(");
        }

        [Fact]
        public void ShouldGenerateAMappingPlanForAOneToOneRelationship()
        {
            string plan = Mapper
                .GetPlanFor<Parent>()
                .ToANew<Parent>();

            plan.ShouldContain("Map Parent -> Parent");
            plan.ShouldContain("mapRecursion(");
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

        internal class Video
        {
            public IEnumerable<VideoPresenter> Presenters { get; set; }
        }

        internal class VideoPresenter
        {
            public Video Video { get; set; }

            public Presenter Presenter { get; set; }
        }

        internal class Presenter
        {
            public IEnumerable<PresenterExpertise> Expertises { get; set; }
        }

        internal class PresenterExpertise
        {
            public Presenter Presenter { get; set; }

            public Expertise Expertise { get; set; }
        }

        internal class Expertise
        {
            public Subject Subject { get; set; }
        }

        internal class Subject
        {
            public IEnumerable<SubjectPresenter> Presenters { get; set; }
        }

        internal class SubjectPresenter
        {
            public Subject Subject { get; set; }

            public Presenter Presenter { get; set; }
        }

        #endregion
    }
}
