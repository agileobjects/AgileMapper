﻿namespace AgileObjects.AgileMapper.UnitTests.Dynamics.Configuration
{
    using AgileMapper.Configuration;
    using Xunit;

    public class WhenConfiguringDynamicMappingIncorrectly
    {
        [Fact]
        public void ShouldErrorIfRedundantSourceSeparatorIsConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .Dynamics
                        .UseMemberNameSeparator("-")
                        .AndWhenMapping
                        .FromDynamics
                        .UseMemberNameSeparator("-");
                }
            });

            configEx.Message.ShouldContain("already");
            configEx.Message.ShouldContain("global");
            configEx.Message.ShouldContain("'-'");
        }

        [Fact]
        public void ShouldErrorIfRedundantGlobalSeparatorIsConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .Dynamics
                        .UseMemberNameSeparator("_");
                }
            });

            configEx.Message.ShouldContain("already");
            configEx.Message.ShouldContain("global");
            configEx.Message.ShouldContain("'_'");
        }

        [Fact]
        public void ShouldErrorIfRedundantSourceElementKeyPartIsConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .FromDynamics
                        .UseElementKeyPattern("_i");
                }
            });

            configEx.Message.ShouldContain("already");
            configEx.Message.ShouldContain("global");
            configEx.Message.ShouldContain("_i");
        }
    }
}
