namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AgileMapper.Configuration;
    using AgileMapper.Extensions.Internal;
    using MoreTestClasses;
    using NetStandardPolyfills;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenApplyingMapperConfigurations : AssemblyScanningTestClassBase
    {
        [Fact]
        public void ShouldApplyAGivenMapperConfiguration()
        {
            TestThenReset(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .UseConfigurations.From<PfiToPfsMapperConfiguration>();

                    PfiToPfsMapperConfiguration.VerifyConfigured(mapper);
                }
            });
        }

        [Fact]
        public void ShouldApplyMapperConfigurationsInAGivenTypeAssembly()
        {
            TestThenReset(() =>
                {
                    using (var mapper = Mapper.CreateNew())
                    {
                        mapper.WhenMapping
                            .UseServiceProvider(t => null)
                            .UseConfigurations.FromAssemblyOf<WhenApplyingMapperConfigurations>();

                        PfiToPfsMapperConfiguration.VerifyConfigured(mapper);
                        PfsToPfiMapperConfiguration.VerifyConfigured(mapper);
                    }
                });
        }

        [Fact]
        public void ShouldProvideRegisteredServicesToMapperConfigurations()
        {
            TestThenReset(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    var mappersByName = new Dictionary<string, IMapper>();

                    mapper.WhenMapping
                        .UseServiceProvider(t => mappersByName)
                        .UseConfigurations.FromAssemblyOf<AnimalBase>();

                    ServiceDictionaryMapperConfiguration
                        .VerifyConfigured(mappersByName)
                        .ShouldBeTrue();
                }
            });
        }

#if !NET_STANDARD
        [Fact]
        public void ShouldApplyMapperConfigurationsFromGivenAssemblies()
        {
            TestThenReset(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    var mappersByName = new Dictionary<string, IMapper>();

                    mapper.WhenMapping
                        .UseServiceProvider(new SingletonServiceProvider(mappersByName))
                        .UseConfigurations.From(AppDomain.CurrentDomain.GetAssemblies());

                    PfiToPfsMapperConfiguration.VerifyConfigured(mapper);
                    PfsToPfiMapperConfiguration.VerifyConfigured(mapper);

                    ServiceDictionaryMapperConfiguration
                        .VerifyConfigured(mappersByName)
                        .ShouldBeTrue();
                }
            });
        }

        [Fact]
        public void ShouldApplyMapperConfigurationsFromCurrentAppDomain()
        {
            TestThenReset(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    var mappersByName = new Dictionary<string, IMapper>();

                    mapper.WhenMapping
                        .UseServiceProvider(new SingletonServiceProvider(mappersByName))
                        .UseConfigurations.FromCurrentAppDomain();

                    PfiToPfsMapperConfiguration.VerifyConfigured(mapper);
                    PfsToPfiMapperConfiguration.VerifyConfigured(mapper);

                    ServiceDictionaryMapperConfiguration
                        .VerifyConfigured(mappersByName)
                        .ShouldBeTrue();
                }
            });
        }
#endif

        [Fact]
        public void ShouldFilterMapperConfigurationsFromGivenAssemblies()
        {
            TestThenReset(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .UseServiceProvider(t => null)
                        .UseConfigurations.From(
                            new[]
                            {
                                typeof(PfiToPfsMapperConfiguration).GetAssembly(),
                                typeof(ServiceDictionaryMapperConfiguration).GetAssembly()
                            },
                            assembly => !assembly.FullName.Contains(nameof(MoreTestClasses)));

                    PfiToPfsMapperConfiguration.VerifyConfigured(mapper);
                    PfsToPfiMapperConfiguration.VerifyConfigured(mapper);
                }
            });
        }

        [Fact]
        public void ShouldApplyMapperConfigurationCallbacks()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var data = new Dictionary<string, object>();

                mapper.WhenMapping
                    .UseServiceProvider(t => data)
                    .UseConfigurations.From<CallbacksMapperConfiguration>();

                var source = new Address { Line1 = "One", Line2 = "Two" };
                var result = mapper.DeepClone(source);

                result.Line1.ShouldBe("One");
                result.Line2.ShouldBe("Two");

                data["SourceAddress"].ShouldBe(source);
                data["TargetAddress"].ShouldBe(result);
            }
        }

        // See https://github.com/agileobjects/AgileMapper/issues/74
        [Fact]
        public void ShouldApplyMapperConfigurationsInOrder()
        {
            var values = new List<int> { 1, 2, 3 };
            var provider = new StubServiceProvider(values);

            var grandParent = new GrandParent
            {
                Child = new Parent { MyChild = new Child() }
            };

            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .UseServiceProvider(provider)
                    .UseConfigurations
                    .FromAssemblyOf<ParentMapperConfiguration>();

                var result = mapper.Map(grandParent).ToANew<GrandParentDto>();

                result.MyChild.ShouldNotBeNull();
                result.MyChild.MyChild.ShouldNotBeNull();
                result.MyChild.MyChild.IsMyCountGtrZero.ShouldBeTrue();
            }
        }

        #region Helper Classes

        public class PfiToPfsMapperConfiguration : MapperConfiguration
        {
            protected override void Configure()
            {
                WhenMapping
                    .From<PublicField<int>>()
                    .ToANew<PublicField<string>>()
                    .Map(ctx => ctx.Source.Value * 2)
                    .To(t => t.Value);

                GetPlanFor<PublicField<int>>().ToANew<PublicField<string>>();
            }

            public static void VerifyConfigured(IMapper mapper)
            {
                var source = new PublicField<int> { Value = 123 };
                var result = mapper.Map(source).ToANew<PublicField<string>>();

                result.Value.ShouldBe(246);
            }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        public class PfsToPfiMapperConfiguration : MapperConfiguration
        {
            protected override void Configure()
            {
                WhenMapping
                    .From<PublicField<string>>()
                    .ToANew<PublicField<int>>()
                    .Map(ctx => ctx.Source.Value + "10")
                    .To(t => t.Value);

                GetPlansFor<PublicField<string>>().To<PublicField<int>>();
            }

            public static void VerifyConfigured(IMapper mapper)
            {
                var source = new PublicField<string> { Value = "10" };
                var result = mapper.Map(source).ToANew<PublicField<int>>();

                result.Value.ShouldBe(1010);
            }
        }

        public class CallbacksMapperConfiguration : MapperConfiguration
        {
            protected override void Configure()
            {
                var dataByKey = GetService<Dictionary<string, object>>();

                if (dataByKey == null)
                {
                    return;
                }

                WhenMapping
                    .From<Address>()
                    .To<Address>()
                    .Before.MappingBegins
                    .Call(ctx => dataByKey["SourceAddress"] = ctx.Source)
                    .And
                    .After.MappingEnds
                    .Call(ctx => dataByKey["TargetAddress"] = ctx.Target);
            }
        }

        public class SingletonServiceProvider
        {
            private readonly Dictionary<Type, object> _objectsByType;

            public SingletonServiceProvider(params object[] services)
            {
                _objectsByType = services.ToDictionary(s => s.GetType());
            }

            public object GetInstance(Type type)
                => _objectsByType.TryGetValue(type, out var service) ? service : null;
        }

        private class GrandParent
        {
            public Parent Child { get; set; }
        }

        private class GrandParentDto
        {
            public ParentDto MyChild { get; set; }
        }

        private class Parent
        {
            public Child MyChild { get; set; }
        }

        private class ParentDto
        {
            public ChildDto MyChild { get; set; }
        }

        private class Child
        {
            public bool IsMyCountGtrZero(ICollection<int> values)
                => values.Count > 0;
        }

        private class ChildDto
        {
            public bool IsMyCountGtrZero { get; set; }
        }

        [ApplyAfter(typeof(ParentMapperConfiguration))]
        private class GrandParentMapperConfiguration : MapperConfiguration
        {
            protected override void Configure()
            {
                WhenMapping
                    .From<GrandParent>()
                    .To<GrandParentDto>()
                    .Map(ctx => ctx.Source.Child)
                    .To(dto => dto.MyChild);
            }
        }

        [ApplyAfter(typeof(ChildMapperConfiguration))]
        private class ParentMapperConfiguration : MapperConfiguration
        {
            protected override void Configure()
                => GetPlansFor<Parent>().To<ParentDto>();
        }

        [ApplyAfter(typeof(ParentMapperConfiguration), typeof(ChildMapperConfiguration))]
        private class CousinMapperConfiguration : MapperConfiguration
        {
            protected override void Configure()
            {
            }
        }

        private class ChildMapperConfiguration : MapperConfiguration
        {
            protected override void Configure()
            {
                WhenMapping
                    .From<Child>()
                    .To<ChildDto>()
                    .Map(ctx => ctx.Source.IsMyCountGtrZero(ctx.GetService<List<int>>())).To(o => o.IsMyCountGtrZero);

                GetPlansFor<Child>().To<ChildDto>();
            }
        }

        #endregion
    }
}
