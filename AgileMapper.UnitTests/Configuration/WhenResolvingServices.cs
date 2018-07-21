namespace AgileObjects.AgileMapper.UnitTests.Configuration
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
    public class WhenResolvingServices
    {
        [Fact]
        public void ShouldUseAConfiguredServiceProvider()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var logger = new Logger();

                mapper.WhenMapping
                    .UseServiceProvider(t => logger);

                mapper.Before
                    .MappingBegins
                    .Call(ctx => ctx.GetService<Logger>().Log("Mapping!"));

                var source = new PublicField<string> { Value = "Logging!" };

                mapper.Map(source).Over(new PublicField<string>());

                logger.EntryCount.ShouldBe(1);
            }
        }

        [Fact]
        public void ShouldUseAConfiguredNamedServiceProvider()
        {
            var logger = new Logger();

            var mappingEx = Should.Throw<MappingException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .UseServiceProvider((t, name) => (name == "Frank") ? logger : null);

                    mapper.Before
                        .MappingBegins
                        .Call(ctx => ctx.GetService<Logger>("Frank").Log("Mapping!"))
                        .And
                        .After
                        .MappingEnds
                        .Call(ctx => ctx.GetService<Logger>().Log("This will be null!"));

                    var source = new PublicField<string> { Value = "Logging!" };

                    mapper.Map(source).Over(new PublicField<string>());
                }
            });

            mappingEx.InnerException.ShouldBeOfType<NullReferenceException>();
            logger.EntryCount.ShouldBe(1);
        }

        #region Helper Classes

        public class Logger
        {
            private readonly List<string> _logs;

            public Logger()
            {
                _logs = new List<string>();
            }

            public int EntryCount => _logs.Count;

            public void Log(string message)
            {
                _logs.Add(message);
            }
        }

        #endregion
    }
}
