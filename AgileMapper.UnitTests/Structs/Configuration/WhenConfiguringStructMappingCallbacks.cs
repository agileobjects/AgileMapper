namespace AgileObjects.AgileMapper.UnitTests.Structs.Configuration
{
    using System;
    using System.Collections.Generic;
    using AgileMapper.Configuration;
    using AgileMapper.Extensions.Internal;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenConfiguringStructMappingCallbacks
    {
        [Fact]
        public void ShouldExecuteAGlobalPreMappingCallback()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var mappedTypes = new List<Type>();

                mapper.Before.MappingBegins
                    .Call((s, t) => mappedTypes.Add(t.GetType()));

                mapper.Map(new { Value = "Bernie" }).OnTo(new PublicTwoFieldsStruct<int, int>());
                mapper.Map(new PublicPropertyStruct<int>()).Over(new PublicPropertyStruct<int>());

                mappedTypes.ShouldNotBeEmpty();
                mappedTypes.ShouldBe(typeof(PublicTwoFieldsStruct<int, int>), typeof(PublicPropertyStruct<int>));
            }
        }

        [Fact]
        public void ShouldExecuteAGlobalPostMappingCallback()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var mappedTargets = new List<object>();

                mapper.After.MappingEnds
                    .Call((s, t) => mappedTargets.Add(t));

                var source = new { Value = "Doobey Do" };

                mapper.Map(source).Over(new PublicPropertyStruct<string>());
                mapper.Map(source).Over(new PublicPropertyStruct<int>());

                mappedTargets.ShouldNotBeEmpty();
                mappedTargets.Count.ShouldBe(2);

                mappedTargets.First().ShouldBeOfType<PublicPropertyStruct<string>>();
                ((PublicPropertyStruct<string>)mappedTargets.First()).Value.ShouldBe("Doobey Do");

                mappedTargets.Second().ShouldBeOfType<PublicPropertyStruct<int>>();
                ((PublicPropertyStruct<int>)mappedTargets.Second()).Value.ShouldBeDefault();
            }
        }

        [Fact]
        public void ShouldErrorIfPreMemberMappingCallbackIsConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .ToANew<PublicPropertyStruct<string>>()
                        .Before
                        .Mapping(pps => pps.Value)
                        .Call((s, pps) => Console.WriteLine("Pre:" + pps.Value));
                }
            });

            configEx.InnerException.ShouldNotBeNull();
            configEx.InnerException.ShouldBeOfType<NotSupportedException>();
        }

        [Fact]
        public void ShouldErrorIfPostMemberMappingCallbackIsConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .ToANew<PublicPropertyStruct<string>>()
                        .After
                        .Mapping(pps => pps.Value)
                        .Call((s, pps, i) => Console.WriteLine("Post: " + pps.Value));
                }
            });

            configEx.InnerException.ShouldNotBeNull();
            configEx.InnerException.ShouldBeOfType<NotSupportedException>();
        }
    }
}