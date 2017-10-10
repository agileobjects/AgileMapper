namespace AgileObjects.AgileMapper.UnitTests.Structs.Configuration
{
    using System;
    using System.Collections.Generic;
    using AgileMapper.Configuration;
    using Shouldly;
    using TestClasses;
    using Xunit;

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

                mapper.Map(new { Value = "Bernie" }).Over(new PublicTwoFieldsStruct<int, int>());
                mapper.Map(new PublicPropertyStruct<int>()).Over(new PublicPropertyStruct<int>());

                mappedTypes.ShouldNotBeEmpty();
                mappedTypes.ShouldBe(typeof(PublicTwoFieldsStruct<int, int>), typeof(PublicPropertyStruct<int>));
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