namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Collections.Generic;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenUnflatteningFromDictionaries
    {
        [Fact]
        public void ShouldPopulateASimpleTypeMember()
        {
            var source = new Dictionary<string, string> { ["Value"] = "Flatten THIS" };
            var result = Mapper.Unflatten(source).To<PublicProperty<string>>();

            result.ShouldNotBeNull();
            result.Value.ShouldBe("Flatten THIS");
        }

        [Fact]
        public void ShouldPopulateASimpleTypeArrayMember()
        {
            var source = new Dictionary<string, object>
            {
                ["Value[0]"] = 1L,
                ["Value[1]"] = 2L,
                ["Value[2]"] = 3L
            };
            var result = Mapper.Unflatten(source).To<PublicProperty<long[]>>();

            result.ShouldNotBeNull();
            result.Value.ShouldBe(1L, 2L, 3L);
        }

        [Fact]
        public void ShouldPopulateAComplexTypeMember()
        {
            var source = new Dictionary<string, string> { ["Value.Value"] = "1234" };
            var result = Mapper.CreateNew().Unflatten(source).To<PublicProperty<PublicField<int>>>();

            result.Value.Value.ShouldBe(1234);
        }

        [Fact]
        public void ShouldPopulateANullableDateTimeOffsetMember()
        {
            var source = new Dictionary<string, string> { ["Value"] = "2018-07-30 14:30:05" };
            var result = Mapper.Unflatten(source).To<PublicProperty<DateTimeOffset?>>();

            result.Value.ShouldNotBeNull();

            // ReSharper disable once PossibleInvalidOperationException
            result.Value.Value.Year.ShouldBe(2018);
            result.Value.Value.Month.ShouldBe(07);
            result.Value.Value.Day.ShouldBe(30);
            result.Value.Value.Hour.ShouldBe(14);
            result.Value.Value.Minute.ShouldBe(30);
            result.Value.Value.Second.ShouldBe(05);
        }

        [Fact]
        public void ShouldPopulateANullNullableInt()
        {
            var source = new Dictionary<string, object>
            {
                ["Value1"] = 123,
                ["Value2"] = null
            };
            var result = Mapper.Unflatten(source).To<PublicTwoFields<int?, int?>>();

            result.Value1.ShouldBe(123);
            result.Value2.ShouldBeNull();
        }

        [Fact]
        public void ShouldFlattenAComplexTypeEnumerableMember()
        {
            var source = new Dictionary<string, string>
            {
                ["Value[0]ProductId"] = "SumminElse"
            };

            var result = Mapper
                .Unflatten(source)
                .To<PublicProperty<IEnumerable<Product>>>(cfg => cfg
                    .ForDictionaries.UseFlattenedMemberNames());

            result.Value.ShouldNotBeNull();
            result.Value.ShouldHaveSingleItem().ProductId.ShouldBe("SumminElse");
        }

        //[Fact]
        //public void ShouldHandleANullComplexTypeEnumerableMemberElement()
        //{
        //    var source = new PublicProperty<IEnumerable<Product>>
        //    {
        //        Value = new Product[] { null }
        //    };
        //    var result = Mapper.Flatten(source).ToDictionary();

        //    result.ShouldNotContainKey("Value[0].ProductId");
        //}

        //[Fact]
        //public void ShouldFlattenARecursiveObjectModel()
        //{
        //    var scienceCommunity = new CommunityViewModel
        //    {
        //        Id = 123,
        //        Name = "Science",
        //        UrlFriendlyName = "science",
        //        MetaTitle = "Dat Science",
        //        MetaDescription = "All about science",
        //        ShortDescription = "Sci",
        //        Status = CommunityStatus.Live,
        //        IsFeatured = true,
        //        DisplayOrder = 1
        //    };

        //    var pipettesCommunity = new CommunityViewModel
        //    {
        //        Id = 563,
        //        Name = "Pipettes",
        //        UrlFriendlyName = "pipettes",
        //        MetaTitle = "Dem Pipettes",
        //        MetaDescription = "All about Pipettes",
        //        ShortDescription = "Pips",
        //        Status = CommunityStatus.Live,
        //        DisplayOrder = 2
        //    };

        //    var biologyTopic = new TopicViewModel
        //    {
        //        Id = 456,
        //        Name = "Biology",
        //        UrlFriendlyName = "biology",
        //        AuthorName = "Richard Dawkins",
        //        AuthorUrlFriendlyName = "richard-dawkins",
        //        MetaTitle = "Dat Biology",
        //        MetaDescription = "Such biology",
        //        ShortDescription = "Such Bio",
        //        Status = TopicStatus.Live,
        //        IsFeatured = true,
        //        DisplayOrder = 1
        //    };

        //    var physicsTopic = new TopicViewModel
        //    {
        //        Id = 789,
        //        Name = "Physics",
        //        UrlFriendlyName = "physics",
        //        AuthorName = "Neil Degrasse Tyson",
        //        AuthorUrlFriendlyName = "neil-degrasse-tyson",
        //        MetaTitle = "Dat Physics",
        //        MetaDescription = "Such physics",
        //        ShortDescription = "Such phs",
        //        Status = TopicStatus.Live,
        //        IsFeatured = true,
        //        DisplayOrder = 1
        //    };

        //    scienceCommunity.Topics.AddRange(new[] { biologyTopic, physicsTopic });
        //    scienceCommunity.Communities.Add(pipettesCommunity);
        //    pipettesCommunity.Topics.Add(biologyTopic);
        //    pipettesCommunity.Communities.Add(scienceCommunity);
        //    biologyTopic.Communities.AddRange(new[] { scienceCommunity, pipettesCommunity });
        //    physicsTopic.Communities.Add(scienceCommunity);

        //    var flattened = Mapper
        //        .Flatten(scienceCommunity)
        //        .ToDictionary();

        //    flattened["Name"].ShouldBe("Science");
        //    flattened["Communities[0].Name"].ShouldBe("Pipettes");
        //    flattened["Communities[0].Topics[0].Name"].ShouldBe("Biology");
        //    flattened["Communities[0].Topics[0].IsFeatured"].ShouldBe(true);
        //    flattened["Topics[0].Name"].ShouldBe("Biology");
        //    flattened["Topics[0].Communities[0].Id"].ShouldBe(123);
        //    flattened["Topics[0].Communities[0].Name"].ShouldBe("Science");
        //    flattened["Topics[0].Communities[1].Name"].ShouldBe("Pipettes");
        //    flattened["Topics[1].Name"].ShouldBe("Physics");
        //    flattened["Topics[1].Status"].ShouldBe(TopicStatus.Live);
        //    flattened["Topics[1].Communities[0].Name"].ShouldBe("Science");
        //}

        //#region Helper Classes

        //public abstract class PageContentViewModelBase
        //{
        //    private string _metaTitle;

        //    private string _metaDescription;

        //    public string Name { get; set; }

        //    public string ShortDescription { get; set; }

        //    public string MetaTitle
        //    {
        //        get => string.IsNullOrEmpty(_metaTitle)
        //            ? $"Message Board topic {Name}"
        //            : _metaTitle;

        //        set => _metaTitle = value;
        //    }

        //    public string MetaDescription
        //    {
        //        get => string.IsNullOrEmpty(_metaDescription)
        //            ? $"Message Board topic {ShortDescription}"
        //            : _metaDescription;

        //        set => _metaDescription = value;
        //    }

        //    public string PageTitle => $"Message Board {Name}";
        //}

        //public class CommunityViewModel : PageContentViewModelBase
        //{
        //    public CommunityViewModel()
        //    {
        //        Topics = new List<TopicViewModel>();
        //        Communities = new List<CommunityViewModel>();
        //    }

        //    public int Id { get; set; }

        //    public int DisplayOrder { get; set; }

        //    public bool IsFeatured { get; set; }

        //    public CommunityStatus? Status { get; set; }

        //    public string UrlFriendlyName { get; set; }

        //    public List<TopicViewModel> Topics { get; set; }

        //    public List<CommunityViewModel> Communities { get; set; }
        //}

        //public class TopicViewModel : PageContentViewModelBase
        //{
        //    public TopicViewModel()
        //    {
        //        Communities = new List<CommunityViewModel>();
        //    }

        //    public int Id { get; set; }

        //    public int DisplayOrder { get; set; }

        //    public bool IsFeatured { get; set; }

        //    public TopicStatus Status { get; set; }

        //    public string UrlFriendlyName { get; set; }

        //    public string AuthorName { get; set; }

        //    public string AuthorUrlFriendlyName { get; set; }

        //    public List<CommunityViewModel> Communities { get; set; }
        //}

        //public enum CommunityStatus
        //{
        //    Undefined = 0,
        //    Live = 1
        //}

        //public enum TopicStatus
        //{
        //    Undefined = 0,
        //    Live = 1
        //}

        //#endregion
    }
}
