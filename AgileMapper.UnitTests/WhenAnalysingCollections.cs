namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Linq;
    using AgileMapper.Extensions;
    using Common;
    using Common.TestClasses;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    [Trait("Category", "Checked")]
    public class WhenAnalysingCollections
    {
        [Fact]
        public void ShouldHandleNullTargetSameTypeCollection()
        {
            var source = new[] { new Person { Name = "Frank" } };
            var collectionData = CollectionData.Create(source, default(Person[]), p => p.Id);

            collectionData.AbsentTargetItems.ShouldBeEmpty();
            collectionData.Intersection.ShouldBeEmpty();
            collectionData.NewSourceItems.SequenceEqual(source).ShouldBeTrue();
        }

        [Fact]
        public void ShouldAnalyseSameTypeCollection()
        {
            var source = new[] { new Product { ProductId = "1" }, new Product { ProductId = "2" } };
            var target = new[] { new Product { ProductId = "2" }, new Product { ProductId = "3" } };
            var collectionData = CollectionData.Create(source, target, p => p.ProductId);

            collectionData.AbsentTargetItems.ShouldHaveSingleItem();
            collectionData.AbsentTargetItems.First().ProductId.ShouldBe("3");
            collectionData.Intersection.ShouldHaveSingleItem();
            collectionData.Intersection.First().Item2.ProductId.ShouldBe("2");
            collectionData.NewSourceItems.ShouldHaveSingleItem();
            collectionData.NewSourceItems.First().ProductId.ShouldBe("1");
        }

        [Fact]
        public void ShouldHandleNullTargetDifferentTypeCollection()
        {
            var source = new[] { new PersonViewModel { Name = "Frank" } };
            var collectionData = CollectionData.Create(source, default(Person[]), pvm => pvm.Id, p => p.Id);

            collectionData.AbsentTargetItems.ShouldBeEmpty();
            collectionData.Intersection.ShouldBeEmpty();
            collectionData.NewSourceItems.SequenceEqual(source).ShouldBeTrue();
        }

        [Fact]
        public void ShouldAnalyseDifferentTypeCollection()
        {
            var idOne = Guid.NewGuid();
            var idTwo = Guid.NewGuid();
            var idThree = Guid.NewGuid();
            var source = new[] { new Person { Id = idOne }, new Person { Id = idTwo } };
            var target = new[] { new PersonViewModel { Id = idTwo }, new PersonViewModel { Id = idThree } };
            var collectionData = CollectionData.Create(source, target, p => p.Id, pvm => pvm.Id);

            collectionData.AbsentTargetItems.ShouldHaveSingleItem();
            collectionData.AbsentTargetItems.First().Id.ShouldBe(idThree);
            collectionData.Intersection.ShouldHaveSingleItem();
            collectionData.Intersection.First().Item2.Id.ShouldBe(idTwo);
            collectionData.NewSourceItems.ShouldHaveSingleItem();
            collectionData.NewSourceItems.First().Id.ShouldBe(idOne);
        }
    }
}
