namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using AgileMapper.Extensions;
    using AgileMapper.Extensions.Internal;
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
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

            var clonedPilot = Mapper.DeepClone(pilot);

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

                var clonedPilot = mapper.DeepClone(pilot);

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
            var order = new Order
            {
                DateCreated = DateTime.Now,
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductId = "Grass" },
                    new OrderItem { ProductId = "Flowers" }
                }
            };

            order.Items.ForEach(item => item.Order = order);

            var result = Mapper.DeepClone(order);

            result.ShouldNotBeSameAs(order);
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

            var clonedJack = Mapper.DeepClone(jack);

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
            var recursorOne = new MultipleRecursor { Name = "One", Ints = new[] { 1, 0 } };
            var recursorTwo = new MultipleRecursor { Name = "Two" };

            var recursorThree = new MultipleRecursor
            {
                Name = "Three",
                ChildRecursor = new MultipleRecursor
                {
                    Name = "Three.ChildRecursor",
                    ChildRecursorArray = new[] { recursorTwo }
                },
                Ints = new[] { 3, 3, 3 }
            };

            var source = new MultipleRecursor
            {
                Name = "Root",
                ChildRecursor = new MultipleRecursor
                {
                    Name = "Root.ChildRecursor",
                    ChildRecursor = recursorOne,
                    Ints = new[] { 1, 2 }
                },
                ChildRecursorArray = new[]
                {
                    new MultipleRecursor { Name = "Root.ChildRecursorArray[0]" },
                    recursorOne,
                    new MultipleRecursor
                    {
                        Name = "Root.ChildRecursorArray[2]",
                        ChildRecursor = recursorThree,
                        Ints = new[] { 1, 2, 3 }
                    }
                },
                ChildRecursors = new List<MultipleRecursor>
                {
                    recursorTwo,
                    new MultipleRecursor
                    {
                        Name = "Root.ChildRecursors[1]",
                        Ints = new[] { 1, 2, 3, 4 }
                    },
                    recursorThree
                },
                Ints = new[] { 1, 1 }
            };

            var result = Mapper.Map(source).ToANew<MultipleRecursor>();

            result.ShouldNotBeNull();
            result.ShouldNotBeSameAs(source);

            result.Name.ShouldBe("Root");

            result.ChildRecursor.ShouldNotBeNull();
            result.ChildRecursor.Name.ShouldBe("Root.ChildRecursor");
            result.ChildRecursor.Ints.ShouldBe(1, 2);

            var clonedRecursorOne = result.ChildRecursor.ChildRecursor;
            clonedRecursorOne.ShouldNotBeNull();
            clonedRecursorOne.ShouldNotBeSameAs(recursorOne);
            clonedRecursorOne.Name.ShouldBe("One");
            clonedRecursorOne.Ints.ShouldBe(1, 0);

            result.ChildRecursorArray.ShouldNotBeNull();
            result.ChildRecursorArray.Length.ShouldBe(3);
            result.ChildRecursorArray.First().Name.ShouldBe("Root.ChildRecursorArray[0]");
            result.ChildRecursorArray.First().Ints.ShouldBeEmpty();
            result.ChildRecursorArray.Second().ShouldBeSameAs(clonedRecursorOne);
            result.ChildRecursorArray.Second().Ints.ShouldBe(1, 0);
            result.ChildRecursorArray.Third().Name.ShouldBe("Root.ChildRecursorArray[2]");
            result.ChildRecursorArray.Third().Ints.ShouldBe(1, 2, 3);

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
            result.ChildRecursors.First().Ints.ShouldBeEmpty();
            result.ChildRecursors.Second().Name.ShouldBe("Root.ChildRecursors[1]");
            result.ChildRecursors.Second().Ints.ShouldBe(1, 2, 3, 4);
            result.ChildRecursors.Third().ShouldBeSameAs(clonedRecursorThree);
            result.ChildRecursors.Third().Ints.ShouldBe(3, 3, 3);

            result.Ints.ShouldBe(1, 1);
        }

        [Fact]
        public void ShouldMapToANewLinkRelationship()
        {
            var titchmarsh = new Presenter();

            var gardening = new Subject
            {
                Presenters = new[]
                {
                    new SubjectPresenter { Presenter = titchmarsh }
                }
            };

            gardening.Presenters.First().Subject = gardening;

            titchmarsh.Expertises = new[]
            {
                new PresenterExpertise
                {
                    Expertise = new Expertise { Subject = gardening },
                    Presenter = titchmarsh
                }
            };

            var flowersAndStuff = new Video
            {
                Presenters = new[]
                {
                    new VideoPresenter
                    {
                        Presenter = titchmarsh
                    }
                }
            };

            flowersAndStuff.Presenters.First().Video = flowersAndStuff;

            var moreFlowersAndStuff = Mapper.DeepClone(flowersAndStuff);

            moreFlowersAndStuff.ShouldNotBeSameAs(flowersAndStuff);

            moreFlowersAndStuff.Presenters.ShouldHaveSingleItem();
            moreFlowersAndStuff.Presenters.First().ShouldNotBeSameAs(flowersAndStuff.Presenters.First());

            moreFlowersAndStuff.Presenters.First().Video.ShouldBeSameAs(moreFlowersAndStuff);

            var moreTitchmarsh = moreFlowersAndStuff.Presenters.First().Presenter;
            moreTitchmarsh.ShouldNotBeSameAs(titchmarsh);
            moreTitchmarsh.Expertises.ShouldHaveSingleItem();
            moreTitchmarsh.Expertises.First().Presenter.ShouldBeSameAs(moreTitchmarsh);

            var moreGardening = moreTitchmarsh.Expertises.First().Expertise.Subject;
            moreGardening.ShouldNotBeSameAs(gardening);
            moreGardening.Presenters.ShouldHaveSingleItem();
            moreGardening.Presenters.First().Presenter.ShouldBeSameAs(moreTitchmarsh);
            moreGardening.Presenters.First().Subject.ShouldBeSameAs(moreGardening);
        }

        [Fact]
        public void ShouldMapNestedMultiplyRecursiveRelationships()
        {
            var recursorOne = new MultipleRecursor { Name = "One" };
            var recursorTwo = new MultipleRecursor { Name = "Two" };

            recursorOne.ChildRecursor = recursorTwo;
            recursorTwo.ChildRecursor = recursorOne;

            var source = new PublicField<MultipleRecursor[]>
            {
                Value = new[] { recursorOne, recursorTwo }
            };

            var result = Mapper.Map(source).ToANew<PublicField<ReadOnlyCollection<MultipleRecursor>>>();

            result.ShouldNotBeNull();
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

        // See https://github.com/agileobjects/AgileMapper/issues/62
        [Fact]
        public void ShouldMapChildOneToManyRecursiveRelationships()
        {
            var sourceLocation1 = new Issue62.Location { Id = 1 };
            var sourceLocation2 = new Issue62.Location { Id = 2 };

            sourceLocation1.LocationPlace = sourceLocation2.LocationPlace = new Issue62.Place
            {
                Locations = new[] { sourceLocation1, sourceLocation2 }
            };

            IEnumerable<Issue62.Location> source = new[] { sourceLocation1, sourceLocation2 };

            var result = Mapper
                .Map(source)
                .ToANew<IEnumerable<Issue62.DtoLocation>>()
                .ToArray();

            result.Length.ShouldBe(2);

            var dtoLocation1 = result.First();
            dtoLocation1.Id.ShouldBe(1);
            dtoLocation1.LocationPlace.ShouldNotBeNull();

            var dtoLocation2 = result.Second();
            dtoLocation2.Id.ShouldBe(2);
            dtoLocation2.LocationPlace.ShouldNotBeNull();
            dtoLocation2.LocationPlace.Locations.Count().ShouldBe(2);

            dtoLocation2.LocationPlace.ShouldBeSameAs(dtoLocation1.LocationPlace);
            dtoLocation2.LocationPlace.Locations.Count().ShouldBe(2);
            dtoLocation2.LocationPlace.Locations.First().ShouldBe(dtoLocation1);
            dtoLocation2.LocationPlace.Locations.Second().ShouldBe(dtoLocation2);
        }

        [Fact]
        public void ShouldMatchConfiguredSourceEnumerablesInRecursiveRelationships()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper
                    .GetPlanFor<IEnumerable<Issue63.Location>>()
                    .ToANew<IEnumerable<Issue63.DtoLocation>>(cfg => cfg
                        .WhenMapping
                        .From<Issue63.Place>()
                        .To<Issue63.DtoPlace>()
                        .Map((p, dto) => p.PlaceLabels.Select(x => x.Label))
                        .To(dto => dto.Labels));

                mapper
                    .GetPlanFor<Issue63.Location>()
                    .ToANew<Issue63.DtoLocation>();

                var sourceLocation = new Issue63.Location { Id = 1 };
                var sourcePlace = new Issue63.Place();
                var sourceLabel = new Issue63.Label { Id = 1, Name = "Mr Label" };
                var sourcePlaceLabel = new Issue63.PlaceLabel { Place = sourcePlace, Label = sourceLabel };

                sourceLocation.Place = sourcePlace;
                sourcePlace.PlaceLabels = new[] { sourcePlaceLabel };
                sourcePlace.Locations = new[] { sourceLocation };

                var source = new[] { sourceLocation };

                var result = mapper
                    .Map<IEnumerable<Issue63.Location>>(source)
                    .ToANew<IEnumerable<Issue63.DtoLocation>>()
                    .ToArray();

                result.Length.ShouldBe(1);
            }
        }

        [Fact]
        public void ShouldUseConfiguredRecursiveDataSources()
        {
            var sourceParent1 = new Parent { Name = "Parent 1", EldestChild = new Child { Name = "Child 1" } };
            sourceParent1.EldestChild.EldestParent = sourceParent1;

            var sourceParent2 = new Parent { Name = "Parent 2", EldestChild = new Child { Name = "Child 2" } };
            sourceParent2.EldestChild.EldestParent = sourceParent2;

            var source = new PublicTwoFields<Parent, Parent>
            {
                Value1 = sourceParent1,
                Value2 = sourceParent2
            };

            using (var mapper = Mapper.CreateNew())
            {
                var result = mapper
                    .Map(source)
                    .ToANew<PublicTwoFields<Parent, Parent>>(cfg => cfg
                        .If(ctx => ctx.Source.Value1.Name == "Parent 1")
                        .Map(ctx => ctx.Source.Value2)
                        .To(t => t.Value1));

                result.Value1.Name.ShouldBe("Parent 2");
                result.Value1.EldestChild.ShouldNotBeNull();
                result.Value1.EldestChild.Name.ShouldBe("Child 2");
                result.Value1.EldestChild.EldestParent.ShouldBeSameAs(result.Value1);

                result.Value2.Name.ShouldBe("Parent 2");
                result.Value2.EldestChild.ShouldNotBeNull();
                result.Value2.EldestChild.Name.ShouldBe("Child 2");
                result.Value2.EldestChild.EldestParent.ShouldBeSameAs(result.Value2);
            }
        }

        // See https://github.com/agileobjects/AgileMapper/issues/77
        [Fact]
        public void ShouldMapMultipleLinkRelationships()
        {
            var warehouse = new Issue77.Warehouse
            {
                Id = 16473,
                Name = "Test Warehouse 16473",
                Description = "The test warehouse"
            };

            var tagForWarehouse = new Issue77.Tag
            {
                Id = 46437
            };

            var warehouseTag = new Issue77.WarehouseTag
            {
                WarehouseId = warehouse.Id,
                Warehouse = warehouse,
                TagId = tagForWarehouse.Id,
                Tag = tagForWarehouse
            };

            var branch = new Issue77.Branch
            {
                Id = 27362,
                Name = "Test Branch 27362",
                Description = "The test branch"
            };

            var tagForBranch = new Issue77.Tag
            {
                Id = 57832
            };

            var branchTag = new Issue77.BranchTag
            {
                BranchId = branch.Id,
                Branch = branch,
                TagId = tagForBranch.Id,
                Tag = tagForBranch
            };

            var warehouseLocation = new Issue77.Location
            {
                Id = 63672,
                Name = "Warehouse Location",
                Description = "Warehouse Street, Warehouse Land"
            };

            var tagForWarehouseLocation = new Issue77.Tag
            {
                Id = 53627
            };

            var warehouseLocationTag = new Issue77.LocationTag
            {
                LocationId = warehouseLocation.Id,
                Location = warehouseLocation,
                TagId = tagForWarehouseLocation.Id,
                Tag = tagForWarehouseLocation
            };

            var branchLocation = new Issue77.Location
            {
                Id = 73726,
                Name = "Branch Location",
                Description = "Branch Street, Branch Land"
            };

            var tagForBranchLocation = new Issue77.Tag
            {
                Id = 53272
            };

            var branchLocationTag = new Issue77.LocationTag
            {
                LocationId = branchLocation.Id,
                Location = branchLocation,
                TagId = tagForBranchLocation.Id,
                Tag = tagForBranchLocation
            };

            var product = new Issue77.Product
            {
                Id = 37638,
                Name = "Test Product",
                Description = "The test product"
            };

            var tagForProduct = new Issue77.Tag
            {
                Id = 58276
            };

            var productTag = new Issue77.ProductTag
            {
                ProductId = product.Id,
                Product = product,
                TagId = tagForProduct.Id,
                Tag = tagForProduct
            };

            var warehouseProduct = new Issue77.WarehouseProduct
            {
                Id = 38376,
                WarehouseId = warehouse.Id,
                Warehouse = warehouse,
                ProductId = product.Id,
                Product = product
            };

            var tagForWarehouseProduct = new Issue77.Tag
            {
                Id = 63463
            };

            var warehouseProductTag = new Issue77.WarehouseProductTag
            {
                WarehouseProductId = warehouseProduct.Id,
                WarehouseProduct = warehouseProduct,
                TagId = tagForWarehouseProduct.Id,
                Tag = tagForWarehouseProduct
            };

            warehouse.BranchId = branch.Id;
            warehouse.Branch = branch;
            warehouse.LocationId = warehouseLocation.Id;
            warehouse.Location = warehouseLocation;
            warehouse.Tags.Add(warehouseTag);
            tagForWarehouse.Warehouses.Add(warehouseTag);
            tagForWarehouseLocation.Locations.Add(warehouseLocationTag);
            warehouseLocation.Tags.Add(warehouseLocationTag);

            branch.Location = branchLocation;
            branch.Warehouses.Add(warehouse);
            branch.Tags.Add(branchTag);
            tagForBranch.Branches.Add(branchTag);
            tagForBranchLocation.Locations.Add(branchLocationTag);
            branchLocation.Tags.Add(branchLocationTag);

            product.Tags.Add(productTag);
            tagForProduct.Products.Add(productTag);

            warehouse.Products.Add(warehouseProduct);
            product.Warehouses.Add(warehouseProduct);
            warehouseProduct.Tags.Add(warehouseProductTag);
            tagForWarehouseProduct.WarehouseProducts.Add(warehouseProductTag);

            Issue77.Warehouse clonedWarehouse;

            using (var mapper = Mapper.CreateNew())
            {
                mapper.GetPlanFor<Issue77.Warehouse>().ToANew<Issue77.Warehouse>();

                clonedWarehouse = mapper.DeepClone(warehouse);

                ((IMapperInternal)mapper).Context.ObjectMapperFactory.RootMappers.ShouldHaveSingleItem();
            }

            clonedWarehouse.ShouldNotBeSameAs(warehouse);
            clonedWarehouse.Id.ShouldBe(16473);
            clonedWarehouse.Name.ShouldBe("Test Warehouse 16473");
            clonedWarehouse.Description.ShouldBe("The test warehouse");
            clonedWarehouse.BranchId.ShouldBe(27362);
            clonedWarehouse.LocationId.ShouldBe(63672);

            var clonedWarehouseTag = clonedWarehouse.Tags.ShouldHaveSingleItem();
            clonedWarehouseTag.Warehouse.ShouldBe(clonedWarehouse);
            var clonedTagForWarehouse = clonedWarehouseTag.Tag.ShouldNotBeNull();
            clonedTagForWarehouse.Warehouses.ShouldHaveSingleItem().ShouldBe(clonedWarehouseTag);

            var clonedBranch = clonedWarehouse.Branch.ShouldNotBeNull();
            clonedBranch.ShouldNotBeSameAs(branch);
            clonedBranch.Id.ShouldBe(27362);
            clonedBranch.Name.ShouldBe("Test Branch 27362");
            clonedBranch.Description.ShouldBe("The test branch");
            clonedBranch.Warehouses.ShouldHaveSingleItem().ShouldBeSameAs(clonedWarehouse);

            var clonedBranchTag = clonedBranch.Tags.ShouldHaveSingleItem();
            clonedBranchTag.Branch.ShouldBeSameAs(clonedBranch);
            var clonedTagForBranch = clonedBranchTag.Tag.ShouldNotBeNull();
            clonedTagForBranch.ShouldNotBeSameAs(tagForBranch);
            clonedTagForBranch.Id.ShouldBe(57832);
            clonedTagForBranch.Branches.ShouldHaveSingleItem().ShouldBeSameAs(clonedBranchTag);

            var clonedWarehouseLocation = clonedWarehouse.Location.ShouldNotBeNull();
            clonedWarehouseLocation.ShouldNotBeSameAs(warehouseLocation);
            clonedWarehouseLocation.Id.ShouldBe(63672);
            clonedWarehouseLocation.Name.ShouldBe("Warehouse Location");
            clonedWarehouseLocation.Description.ShouldBe("Warehouse Street, Warehouse Land");

            var clonedWarehouseLocationTag = clonedWarehouseLocation.Tags.ShouldHaveSingleItem();
            clonedWarehouseLocationTag.Location.ShouldBeSameAs(clonedWarehouseLocation);
            var clonedTagForWarehouseLocation = clonedWarehouseLocationTag.Tag.ShouldNotBeNull();
            clonedTagForWarehouseLocation.ShouldNotBeSameAs(tagForWarehouseLocation);
            clonedTagForWarehouseLocation.Id.ShouldBe(53627);
            clonedTagForWarehouseLocation.Locations.ShouldHaveSingleItem().ShouldBeSameAs(clonedWarehouseLocationTag);

            var clonedBranchLocation = clonedBranch.Location.ShouldNotBeNull();
            clonedBranchLocation.ShouldNotBeSameAs(branchLocation);
            clonedBranchLocation.Id.ShouldBe(73726);
            clonedBranchLocation.Name.ShouldBe("Branch Location");
            clonedBranchLocation.Description.ShouldBe("Branch Street, Branch Land");

            var clonedBranchLocationTag = clonedBranchLocation.Tags.ShouldHaveSingleItem();
            clonedBranchLocationTag.Location.ShouldBeSameAs(clonedBranchLocation);
            var clonedTagForBranchLocation = clonedBranchLocationTag.Tag.ShouldNotBeNull();
            clonedTagForBranchLocation.ShouldNotBeSameAs(tagForBranchLocation);
            clonedTagForBranchLocation.Id.ShouldBe(53272);
            clonedTagForBranchLocation.Locations.ShouldHaveSingleItem().ShouldBeSameAs(clonedBranchLocationTag);

            var clonedWarehouseProduct = clonedWarehouse.Products.ShouldHaveSingleItem();
            clonedWarehouseProduct.ShouldNotBeSameAs(warehouseProduct);
            clonedWarehouseProduct.WarehouseId.ShouldBe(16473);
            clonedWarehouseProduct.Warehouse.ShouldNotBeNull().ShouldBeSameAs(clonedWarehouse);

            var clonedWarehouseProductTag = clonedWarehouseProduct.Tags.ShouldHaveSingleItem();
            clonedWarehouseProductTag.WarehouseProduct.ShouldBeSameAs(clonedWarehouseProduct);
            var clonedTagForWarehouseProduct = clonedWarehouseProductTag.Tag.ShouldNotBeNull();
            clonedTagForWarehouseProduct.ShouldNotBeSameAs(tagForWarehouseProduct);
            clonedTagForWarehouseProduct.Id.ShouldBe(63463);
            clonedTagForWarehouseProduct.WarehouseProducts.ShouldHaveSingleItem().ShouldBeSameAs(clonedWarehouseProductTag);
            clonedWarehouseProduct.ProductId.ShouldBe(37638);

            var clonedProduct = clonedWarehouseProduct.Product.ShouldNotBeNull();
            clonedProduct.ShouldNotBeSameAs(product);
            clonedProduct.Id.ShouldBe(37638);
            clonedProduct.Name.ShouldBe("Test Product");
            clonedProduct.Description.ShouldBe("The test product");
            clonedProduct.Warehouses.ShouldHaveSingleItem().ShouldBeSameAs(clonedWarehouseProduct);

            var clonedProductTag = clonedProduct.Tags.ShouldHaveSingleItem();
            clonedProductTag.Product.ShouldBeSameAs(clonedProduct);
            var clonedTagForProduct = clonedProductTag.Tag.ShouldNotBeNull();
            clonedTagForProduct.ShouldNotBeSameAs(tagForProduct);
            clonedTagForProduct.Id.ShouldBe(58276);
            clonedTagForProduct.Products.ShouldHaveSingleItem().ShouldBeSameAs(clonedProductTag);
        }

        [Fact]
        public void ShouldNotMapANullParentMember()
        {
            var category = new CategoryEntity { Name = "Standalone" };
            var clonedCategory = category.DeepClone();

            clonedCategory.Parent.ShouldBeNull();
        }

        [Fact]
        public void ShouldNotMapAnEmptyNestedRecursiveParentMember()
        {
            var source = new
            {
                Value = new CategoryDto { Id = 123, Name = "Child", ParentId = 456 }
            };
            var result = Mapper.Map(source).ToANew<PublicField<CategoryEntity>>();

            result.Value.Id.ShouldBeDefault();
            result.Value.Name.ShouldBe("Child");
            result.Value.ParentId.ShouldBe(456);
            result.Value.Parent.ShouldBeNull();
        }

        [Fact]
        public void ShouldPerformRepeatedComplexTypeMemberAndElementMappings()
        {
            var c = new Issue115.C { Id = 222 };

            var source = new Issue115.A
            {
                Id = 1,
                B = new Issue115.B
                {
                    Id = 11,
                    Cs = new[]
                    {
                        new Issue115.C
                        {
                            Id = 111,
                            Parent = c
                        }
                    }
                },
                C = new Issue115.C
                {
                    Id = 12,
                    Parent = c
                }
            };

            var result = Mapper.Map(source).ToANew<Issue115.ADto>();

            result.Id.ShouldBe(1);

            result.B.ShouldNotBeNull();
            result.B.Id.ShouldBe(11);
            result.B.Cs.ShouldHaveSingleItem();
            result.B.Cs[0].Parent.ShouldNotBeNull();
            result.B.Cs[0].Parent.Id.ShouldBe(222);
            result.B.Cs[0].Parent.Parent.ShouldBeNull();

            result.C.ShouldNotBeNull();
            result.C.Id.ShouldBe(12);
            result.C.Parent.ShouldNotBeNull();
            result.C.Parent.Id.ShouldBe(222);
            result.C.Parent.Parent.ShouldBeNull();
        }

        [Fact]
        public void ShouldGenerateAMappingPlanForLinkRelationships()
        {
            string plan = Mapper.GetPlanFor<Video>().Over<Video>();

            plan.ShouldNotBeNull();
            plan.ShouldContain("WhenMappingCircularReferences.Video -> WhenMappingCircularReferences.Video");
            plan.ShouldContain(".MapRepeated(");
        }

        [Fact]
        public void ShouldGenerateAMappingPlanForAOneToOneRelationship()
        {
            string plan = Mapper
                .GetPlanFor<Parent>()
                .ToANew<Parent>();

            plan.ShouldContain("Map Parent -> Parent");
            plan.ShouldContain(".MapRepeated(");
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

            public ICollection<int> Ints { get; set; }
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

        public static class Issue62
        {
            public class Location
            {
                public int Id { get; set; }

                public Place LocationPlace { get; set; }
            }

            public class Place
            {
                public IEnumerable<Location> Locations { get; set; }
            }

            public class DtoLocation
            {
                public int Id { get; set; }

                public DtoPlace LocationPlace { get; set; }
            }

            public class DtoPlace
            {
                public IEnumerable<DtoLocation> Locations { get; set; }
            }
        }

        public static class Issue63
        {
            public class Location
            {
                public int Id { get; set; }

                public Place Place { get; set; }
            }
            public class Place
            {
                public IEnumerable<PlaceLabel> PlaceLabels { get; set; }

                public IEnumerable<Location> Locations { get; set; }
            }

            public class PlaceLabel
            {
                public virtual Label Label { get; set; }

                public virtual Place Place { get; set; }
            }

            public class Label
            {
                public int Id { get; set; }

                public string Name { get; set; }
            }

            public class DtoLocation
            {
                public int Id { get; set; }

                // ReSharper disable once MemberHidesStaticFromOuterClass
                public DtoPlace Place { get; set; }
            }

            public class DtoPlace
            {
                public IEnumerable<DtoLabel> Labels { get; set; }

                public IEnumerable<DtoLocation> Locations { get; set; }
            }

            public class DtoLabel
            {
                public int Id { get; set; }

                public string Name { get; set; }
            }
        }

        public static class Issue77
        {
            internal class Tag : EntityBase
            {
                public string Name { get; set; }

                public string Description { get; set; }

                public HashSet<BranchTag> Branches { get; set; } = new HashSet<BranchTag>();

                public HashSet<LocationTag> Locations { get; set; } = new HashSet<LocationTag>();

                public HashSet<WarehouseTag> Warehouses { get; set; } = new HashSet<WarehouseTag>();

                public HashSet<ProductTag> Products { get; set; } = new HashSet<ProductTag>();

                public HashSet<MovementTag> Movements { get; set; } = new HashSet<MovementTag>();

                public HashSet<AssociateTag> Associates { get; set; } = new HashSet<AssociateTag>();

                public HashSet<WarehouseProductTag> WarehouseProducts { get; set; } = new HashSet<WarehouseProductTag>();
            }

            internal class BranchTag
            {
                public int TagId { get; set; }

                public Tag Tag { get; set; }

                public int BranchId { get; set; }

                public Branch Branch { get; set; }
            }

            internal class Branch : EntityBase
            {
                public string Name { get; set; }

                public string Description { get; set; }

                public Location Location { get; set; }

                public HashSet<Warehouse> Warehouses { get; set; } = new HashSet<Warehouse>();

                public HashSet<BranchTag> Tags { get; set; } = new HashSet<BranchTag>();
            }

            internal class LocationTag
            {
                public int TagId { get; set; }

                public Tag Tag { get; set; }

                public int LocationId { get; set; }

                public Location Location { get; set; }
            }

            internal class Location : EntityBase
            {
                public string Name { get; set; }

                public string Description { get; set; }

                public HashSet<LocationTag> Tags { get; set; } = new HashSet<LocationTag>();
            }

            internal class WarehouseTag
            {
                public int TagId { get; set; }

                public Tag Tag { get; set; }

                public int WarehouseId { get; set; }

                public Warehouse Warehouse { get; set; }
            }

            internal class Warehouse : EntityBase
            {
                public string Name { get; set; }

                public string Description { get; set; }

                public int BranchId { get; set; }

                public Branch Branch { get; set; }

                public int LocationId { get; set; }

                public Location Location { get; set; }

                public HashSet<WarehouseProduct> Products { get; set; } = new HashSet<WarehouseProduct>();

                public HashSet<WarehouseTag> Tags { get; set; } = new HashSet<WarehouseTag>();
            }

            internal class ProductTag
            {
                public int TagId { get; set; }

                public Tag Tag { get; set; }

                public int ProductId { get; set; }

                public Product Product { get; set; }
            }

            internal class Product : EntityBase
            {
                public string Name { get; set; }

                public string Description { get; set; }

                public HashSet<WarehouseProduct> Warehouses { get; set; } = new HashSet<WarehouseProduct>();

                public HashSet<ProductTag> Tags { get; set; } = new HashSet<ProductTag>();
            }

            internal class MovementTag
            {
                public int TagId { get; set; }

                public Tag Tag { get; set; }

                public int MovementId { get; set; }

                public Movement Movement { get; set; }
            }

            internal class Movement : EntityBase
            {
                public string Name { get; set; }

                public string Description { get; set; }

                public HashSet<MovementTag> Tags { get; set; } = new HashSet<MovementTag>();
            }

            internal class AssociateTag
            {
                public int TagId { get; set; }

                public Tag Tag { get; set; }

                public int AssociateId { get; set; }

                public Associate Associate { get; set; }
            }

            internal class Associate : EntityBase
            {
                public string Name { get; set; }

                public string Description { get; set; }

                public HashSet<AssociateTag> Tags { get; set; } = new HashSet<AssociateTag>();
            }

            internal class WarehouseProductTag
            {
                public int TagId { get; set; }

                public Tag Tag { get; set; }

                public int WarehouseProductId { get; set; }

                public WarehouseProduct WarehouseProduct { get; set; }
            }

            internal class WarehouseProduct : EntityBase
            {
                public int WarehouseId { get; set; }

                public Warehouse Warehouse { get; set; }

                public int ProductId { get; set; }

                public Product Product { get; set; }

                public HashSet<WarehouseProductTag> Tags { get; set; } = new HashSet<WarehouseProductTag>();
            }
        }

        public static class Issue115
        {
            public class A
            {
                public int Id { get; set; }

                public B B { get; set; }

                public C C { get; set; }
            }

            public class B
            {
                public int Id { get; set; }

                public C[] Cs { get; set; }
            }

            public class C
            {
                public int Id { get; set; }

                public C Parent { get; set; }
            }

            public class ADto
            {
                public int Id { get; set; }

                // ReSharper disable MemberHidesStaticFromOuterClass
                public BDto B { get; set; }

                public CDto C { get; set; }
                // ReSharper restore MemberHidesStaticFromOuterClass
            }

            public class BDto
            {
                public int Id { get; set; }

                public CDto[] Cs { get; set; }
            }

            public class CDto
            {
                public int Id { get; set; }

                public CDto Parent { get; set; }
            }
        }

        #endregion
    }
}
