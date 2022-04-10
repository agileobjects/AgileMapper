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
    // See https://github.com/agileobjects/AgileMapper/issues/212#issuecomment-813390188
    public class WhenAccessingDisposedMappers
    {
        [Fact, Trait("Category", "Checked")]
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

        [Fact, Trait("Category", "Checked")]
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

        [Fact, Trait("Category", "Checked")]
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

        [Fact, Trait("Category", "Checked")]
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

        [Fact, Trait("Category", "Checked")]
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

        [Fact, Trait("Category", "Checked")]
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

        [Fact, Trait("Category", "Checked")]
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

        [Fact, Trait("Category", "Checked")]
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

        [Fact, Trait("Category", "Checked")]
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

        [Fact, Trait("Category", "Checked")]
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

        [Fact, Trait("Category", "Checked")]
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

        [Fact, Trait("Category", "Checked")]
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

        [Fact, Trait("Category", "Checked")]
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

        [Fact, Trait("Category", "Checked")]
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

        [Fact, Trait("Category", "Checked")]
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

        [Fact, Trait("Category", "Checked")]
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

        [Fact, Trait("Category", "Checked")]
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

        [Fact, Trait("Category", "Checked")]
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

        [Fact, Trait("Category", "Checked")]
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

        [Fact, Trait("Category", "Checked")]
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

        [Fact, Trait("Category", "Checked")]
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
