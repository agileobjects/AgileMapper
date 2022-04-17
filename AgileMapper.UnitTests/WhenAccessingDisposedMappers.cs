namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Linq;
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
    // See https://github.com/agileobjects/AgileMapper/issues/212#issuecomment-813390188
    public class WhenAccessingDisposedMappers
    {
        [Fact]
        public void ShouldErrorOnConfigurationStartIfDisposed()
        {
            IMapper mapper;

            using (mapper = Mapper.CreateNew())
            {
            }

            Should.Throw<ObjectDisposedException>(() =>
            {
                mapper.WhenMapping
                    .FromDictionaries
                    .UseFlattenedTargetMemberNames();
            });
        }

        [Fact]
        public void ShouldErrorOnBeforeEventConfigurationIfDisposed()
        {
            IMapper mapper;

            using (mapper = Mapper.CreateNew())
            {
            }

            Should.Throw<ObjectDisposedException>(() =>
            {
                mapper.Before
                    .CreatingInstances
                    .Call(ctx => Console.WriteLine("Before Creating!"));
            });
        }

        [Fact]
        public void ShouldErrorOnAfterEventConfigurationIfDisposed()
        {
            IMapper mapper;

            using (mapper = Mapper.CreateNew())
            {
            }

            Should.Throw<ObjectDisposedException>(() =>
            {
                mapper.After
                    .CreatingInstances
                    .Call(ctx => Console.WriteLine("After Creating!"));
            });
        }

        [Fact]
        public void ShouldErrorOnSampleInstancePlanRetrievalIfDisposed()
        {
            IMapper mapper;

            using (mapper = Mapper.CreateNew())
            {
            }

            Should.Throw<ObjectDisposedException>(() =>
            {
                mapper.GetPlanFor(new { Value = 123 }).ToANew<PublicField<int>>();
            });
        }

        [Fact]
        public void ShouldErrorOnSampleInstancePlanSetRetrievalIfDisposed()
        {
            IMapper mapper;

            using (mapper = Mapper.CreateNew())
            {
            }

            Should.Throw<ObjectDisposedException>(() =>
            {
                mapper.GetPlansFor(new { Value = 123 }).To<PublicField<int>>();
            });
        }

        [Fact]
        public void ShouldErrorOnPlanRetrievalIfDisposed()
        {
            IMapper mapper;

            using (mapper = Mapper.CreateNew())
            {
            }

            Should.Throw<ObjectDisposedException>(() =>
            {
                mapper.GetPlanFor<PublicProperty<int>>().ToANew<PublicField<int>>();
            });
        }

        [Fact]
        public void ShouldErrorOnPlanSetRetrievalIfDisposed()
        {
            IMapper mapper;

            using (mapper = Mapper.CreateNew())
            {
            }

            Should.Throw<ObjectDisposedException>(() =>
            {
                mapper.GetPlansFor<PublicProperty<int>>().To<PublicField<int>>();
            });
        }

        [Fact]
        public void ShouldErrorOnProjectionPlanRetrievalIfDisposed()
        {
            IMapper mapper;

            using (mapper = Mapper.CreateNew())
            {
            }

            Should.Throw<ObjectDisposedException>(() =>
            {
                mapper.GetPlanForProjecting(Enumerable.Range(1, 10)
                    .AsQueryable()).To<PublicField<int>>();
            });
        }

        [Fact]
        public void ShouldErrorOnAllPlansRetrievalIfDisposed()
        {
            IMapper mapper;

            using (mapper = Mapper.CreateNew())
            {
                mapper.GetPlansFor<PublicField<int>>().To<PublicField<int>>();
                mapper.GetPlanFor<PublicProperty<int>>().ToANew<PublicCtor<int>>();
            }

            Should.Throw<ObjectDisposedException>(() =>
            {
                mapper.GetPlansInCache();
            });
        }

        [Fact]
        public void ShouldErrorOnAllPlanExpressionsRetrievalIfDisposed()
        {
            IMapper mapper;

            using (mapper = Mapper.CreateNew())
            {
                mapper.GetPlansFor<PublicField<int>>().To<PublicField<int>>();
                mapper.GetPlanFor<PublicProperty<int>>().ToANew<PublicCtor<int>>();
            }

            Should.Throw<ObjectDisposedException>(() =>
            {
                mapper.GetPlanExpressionsInCache();
            });
        }

        [Fact]
        public void ShouldErrorOnPlanCheckIfDisposed()
        {
            IMapper mapper;

            using (mapper = Mapper.CreateNew())
            {
            }

            Should.Throw<ObjectDisposedException>(() =>
            {
                mapper.ThrowNowIfAnyMappingPlanIsIncomplete();
            });
        }

        [Fact]
        public void ShouldErrorOnCreateNewIfDisposed()
        {
            IMapper mapper;

            using (mapper = Mapper.CreateNew())
            {
            }

            Should.Throw<ObjectDisposedException>(() =>
            {
                mapper.Map(new PublicField<int> { Value = 123 })
                      .ToANew<PublicProperty<string>>();
            });
        }

        [Fact]
        public void ShouldErrorOnMergeIfDisposed()
        {
            IMapper mapper;

            using (mapper = Mapper.CreateNew())
            {
            }

            Should.Throw<ObjectDisposedException>(() =>
            {
                mapper.Map(new PublicField<int> { Value = 123 })
                      .OnTo(new PublicProperty<string>());
            });
        }

        [Fact]
        public void ShouldErrorOnOverwriteIfDisposed()
        {
            IMapper mapper;

            using (mapper = Mapper.CreateNew())
            {
            }

            Should.Throw<ObjectDisposedException>(() =>
            {
                mapper.Map(new PublicField<int> { Value = 123 })
                      .Over(new PublicProperty<string> { Value = "456" });
            });
        }

        [Fact]
        public void ShouldErrorOnProjectionIfDisposed()
        {
            IMapper mapper;

            using (mapper = Mapper.CreateNew())
            {
            }

            Should.Throw<ObjectDisposedException>(() =>
            {
                Enumerable
                    .Range(1, 10)
                    .Select(i => new PublicField<int> { Value = i })
                    .AsQueryable()
                    .ProjectUsing(mapper)
                    .To<PublicField<int>>();
            });
        }

        [Fact]
        public void ShouldErrorOnDeepCloneIfDisposed()
        {
            IMapper mapper;

            using (mapper = Mapper.CreateNew())
            {
            }

            Should.Throw<ObjectDisposedException>(() =>
            {
                mapper.DeepClone(new PublicField<int> { Value = 123 });
            });
        }

        [Fact]
        public void ShouldErrorOnInlineConfiguredDeepCloneIfDisposed()
        {
            IMapper mapper;

            using (mapper = Mapper.CreateNew())
            {
            }

            Should.Throw<ObjectDisposedException>(() =>
            {
                mapper.DeepClone(new PublicField<int> { Value = 123 }, cfg => cfg
                    .CreateInstancesOf<PublicField<int>>()
                    .Using(ctx => new PublicField<int> { Value = 456 })
                );
            });
        }

        [Fact]
        public void ShouldErrorOnFlattenIfDisposed()
        {
            IMapper mapper;

            using (mapper = Mapper.CreateNew())
            {
            }

            Should.Throw<ObjectDisposedException>(() =>
            {
                mapper.Flatten(new PublicField<int> { Value = 123 });
            });
        }

        [Fact]
        public void ShouldErrorOnUnflattenIfDisposed()
        {
            IMapper mapper;

            using (mapper = Mapper.CreateNew())
            {
            }

            Should.Throw<ObjectDisposedException>(() =>
            {
                mapper.Unflatten(new StringKeyedDictionary<object>());
            });
        }

        [Fact]
        public void ShouldErrorOnQueryStringUnflattenIfDisposed()
        {
            IMapper mapper;

            using (mapper = Mapper.CreateNew())
            {
            }

            Should.Throw<ObjectDisposedException>(() =>
            {
                mapper.Unflatten(QueryString.Parse("key=value"));
            });
        }

        [Fact]
        public void ShouldErrorOnCloneSelfIfDisposed()
        {
            IMapper mapper;

            using (mapper = Mapper.CreateNew())
            {
            }

            Should.Throw<ObjectDisposedException>(() =>
            {
                mapper.CloneSelf();
            });
        }
    }
}
