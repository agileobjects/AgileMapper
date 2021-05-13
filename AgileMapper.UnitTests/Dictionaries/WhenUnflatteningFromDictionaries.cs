namespace AgileObjects.AgileMapper.UnitTests.Dictionaries
{
    using System;
    using System.Collections.Generic;
    using Common;
    using Common.TestClasses;
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
            var source = new Dictionary<string, string> { ["Value"] = "Unflatten THIS" };
            var result = Mapper.Unflatten(source).To<PublicProperty<string>>();

            result.ShouldNotBeNull();
            result.Value.ShouldBe("Unflatten THIS");
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

        [Fact]
        public void ShouldPopulateARecursiveObjectModel()
        {
            var source = new Dictionary<string, object>
            {
                ["Id"] = 123,
                ["Name"] = "Science",
                ["UrlFriendlyName"] = "Science",
                ["MetaTitle"] = "Dat Science",
                ["MetaDescription"] = "All about science",
                ["ShortDescription"] = "Sci",
                ["Status"] = CommunityStatus.Live,
                ["IsFeatured"] = true,
                ["DisplayOrder"] = 1,

                ["Topics[0].Id"] = 456,
                ["Topics[0].Name"] = "Biology",
                ["Topics[0].UrlFriendlyName"] = "biology",
                ["Topics[0].AuthorName"] = "Richard Dawkins",
                ["Topics[0].AuthorUrlFriendlyName"] = "richard-dawkins",
                ["Topics[0].MetaTitle"] = "Dat Biology",
                ["Topics[0].MetaDescription"] = "Such biology",
                ["Topics[0].ShortDescription"] = "Such bio",
                ["Topics[0].Status"] = TopicStatus.Live,
                ["Topics[0].IsFeatured"] = true,
                ["Topics[0].DisplayOrder"] = 1,

                ["Topics[1].Id"] = 789,
                ["Topics[1].Name"] = "Physics",
                ["Topics[1].UrlFriendlyName"] = "physics",
                ["Topics[1].AuthorName"] = "Neil Degrasse Tyson",
                ["Topics[1].AuthorUrlFriendlyName"] = "neil-degrasse-tyson",
                ["Topics[1].MetaTitle"] = "Dat Physics",
                ["Topics[1].MetaDescription"] = "Such physics",
                ["Topics[1].ShortDescription"] = "Such phs",
                ["Topics[1].Status"] = TopicStatus.Live,
                ["Topics[1].IsFeatured"] = true,
                ["Topics[1].DisplayOrder"] = 1,

                ["Communities[0].Id"] = 563,
                ["Communities[0].Name"] = "Pipettes",
                ["Communities[0].UrlFriendlyName"] = "pipettes",
                ["Communities[0].MetaTitle"] = "Dem Pipettes",
                ["Communities[0].MetaDescription"] = "All about Pipettes",
                ["Communities[0].ShortDescription"] = "Pips",
                ["Communities[0].Status"] = CommunityStatus.Live,
                ["Communities[0].DisplayOrder"] = 2
            };

            var scienceCommunity = Mapper
                .Unflatten(source)
                .To<CommunityViewModel>();

            scienceCommunity.Name.ShouldBe("Science");
            scienceCommunity.Communities[0].Name.ShouldBe("Pipettes");
            scienceCommunity.Communities[0].DisplayOrder.ShouldBe(2);
            scienceCommunity.Communities[0].Topics.ShouldBeEmpty();
            scienceCommunity.Topics[0].Name.ShouldBe("Biology");
            scienceCommunity.Topics[1].Status.ShouldBe(TopicStatus.Live);
            scienceCommunity.Topics[0].Communities.ShouldBeEmpty();
            scienceCommunity.Topics[1].Name.ShouldBe("Physics");
            scienceCommunity.Topics[1].ShortDescription.ShouldBe("Such phs");
            scienceCommunity.Topics[1].Communities.ShouldBeEmpty();
        }

        #region Helper Classes

        public abstract class PageContentViewModelBase
        {
            private string _metaTitle;

            private string _metaDescription;

            public string Name { get; set; }

            public string ShortDescription { get; set; }

            public string MetaTitle
            {
                get => string.IsNullOrEmpty(_metaTitle)
                    ? $"Message Board topic {Name}"
                    : _metaTitle;

                set => _metaTitle = value;
            }

            public string MetaDescription
            {
                get => string.IsNullOrEmpty(_metaDescription)
                    ? $"Message Board topic {ShortDescription}"
                    : _metaDescription;

                set => _metaDescription = value;
            }

            public string PageTitle => $"Message Board {Name}";
        }

        public class CommunityViewModel : PageContentViewModelBase
        {
            public CommunityViewModel()
            {
                Topics = new List<TopicViewModel>();
                Communities = new List<CommunityViewModel>();
            }

            public int Id { get; set; }

            public int DisplayOrder { get; set; }

            public bool IsFeatured { get; set; }

            public CommunityStatus? Status { get; set; }

            public string UrlFriendlyName { get; set; }

            public List<TopicViewModel> Topics { get; set; }

            public List<CommunityViewModel> Communities { get; set; }
        }

        public class TopicViewModel : PageContentViewModelBase
        {
            public TopicViewModel()
            {
                Communities = new List<CommunityViewModel>();
            }

            public int Id { get; set; }

            public int DisplayOrder { get; set; }

            public bool IsFeatured { get; set; }

            public TopicStatus Status { get; set; }

            public string UrlFriendlyName { get; set; }

            public string AuthorName { get; set; }

            public string AuthorUrlFriendlyName { get; set; }

            public List<CommunityViewModel> Communities { get; set; }
        }

        public enum CommunityStatus
        {
            Undefined = 0,
            Live = 1
        }

        public enum TopicStatus
        {
            Undefined = 0,
            Live = 1
        }

        #endregion
    }
}
