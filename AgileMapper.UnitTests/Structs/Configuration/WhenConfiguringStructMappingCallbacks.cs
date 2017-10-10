namespace AgileObjects.AgileMapper.UnitTests.Structs.Configuration
{
    using System;
    using AgileMapper.Configuration;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringStructMappingCallbacks
    {
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