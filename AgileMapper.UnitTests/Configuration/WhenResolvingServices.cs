namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using System.Collections.Generic;
    using AgileMapper.Configuration;
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
                        .UseServiceProvider((t, name) => (name == "Frank") ? logger : throw new NotSupportedException("NO!"));

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

            logger.EntryCount.ShouldBe(1);
            mappingEx.InnerException.ShouldBeOfType<NotSupportedException>();

            // ReSharper disable once PossibleNullReferenceException
            mappingEx.InnerException.Message.ShouldBe("NO!");
        }

        [Fact]
        public void ShouldUseAConfiguredServiceProviderObjectGetService()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var logger = new Logger();
                var serviceProvider = new GetServiceServiceProvider(logger);

                mapper.WhenMapping.UseServiceProvider(serviceProvider);

                mapper.Before
                    .MappingBegins
                    .Call(ctx => ctx.GetService<Logger>().Log("Mapping!"));

                var source = new PublicField<string> { Value = "Logging!" };

                mapper.Map(source).Over(new PublicField<string>());

                logger.EntryCount.ShouldBe(1);
            }
        }

        [Fact]
        public void ShouldUseAConfiguredServiceProviderObjectGetNamedService()
        {
            var logger = new Logger();
            var serviceProvider = new GetServiceWithNameServiceProvider(logger, "Dee");

            var mappingEx = Should.Throw<MappingException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping.UseServiceProvider(serviceProvider);

                    mapper.Before
                        .MappingBegins
                        .Call(ctx => ctx.GetService<Logger>("Dee").Log("Mapping!"))
                        .And
                        .After
                        .MappingEnds
                        .Call(ctx => ctx.GetService<Logger>().Log("Unnamed not supported!"));

                    var source = new PublicField<string> { Value = "Logging!" };

                    mapper.Map(source).Over(new PublicField<string>());
                }
            });

            logger.EntryCount.ShouldBe(1);
            mappingEx.InnerException.ShouldBeOfType<NotSupportedException>();

            // ReSharper disable once PossibleNullReferenceException
            mappingEx.InnerException.Message.ShouldBe("Dee");
        }

        [Fact]
        public void ShouldUseAConfiguredServiceProviderObjectGetInstanceService()
        {
            var logger = new Logger();
            var serviceProvider = new GetInstancesServiceProvider(logger, "Mac");

            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping.UseServiceProvider(serviceProvider);

                mapper.Before
                    .MappingBegins
                    .Call(ctx => ctx.GetService<Logger>("Mac").Log("Mapping!"))
                    .And
                    .After
                    .MappingEnds
                    .Call(ctx => ctx.GetService<Logger>().Log("More mapping!"));

                var source = new PublicField<string> { Value = "Logging!" };

                mapper.Map(source).Over(new PublicField<string>());
            }

            logger.EntryCount.ShouldBe(2);
        }

        [Fact]
        public void ShouldUseAConfiguredServiceProviderObjectResolveService()
        {
            var logger = new Logger();
            var serviceProvider = new ResolveServiceProvider(logger, "Charlie");

            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping.UseServiceProvider(serviceProvider);

                mapper.Before
                    .MappingBegins
                    .Call(ctx => ctx.GetService<Logger>("Charlie").Log("Mapping!"))
                    .And
                    .After
                    .MappingEnds
                    .Call(ctx => ctx.GetService<Logger>().Log("More mapping!"));

                var source = new PublicField<string> { Value = "Logging!" };

                mapper.Map(source).Over(new PublicField<string>());
            }

            logger.EntryCount.ShouldBe(2);
        }

        [Fact]
        public void ShouldExposeAConfiguredServiceProvider()
        {
            var logger = new Logger();
            var serviceProvider = new ResolveServiceProvider(logger, "Charlie");

            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping.UseServiceProvider(serviceProvider);

                mapper.Before
                    .MappingBegins
                    .Call(ctx =>
                    {
                        var log = (Logger)ctx
                            .GetServiceProvider<ResolveServiceProvider>()
                            .GetInstance(typeof(Logger));

                        log.Log("Logged!");
                    });

                var source = new PublicField<string> { Value = "Logging!" };

                mapper.Map(source).Over(new PublicField<string>());
            }

            logger.EntryCount.ShouldBe(1);
        }

        [Fact]
        public void ShouldErrorIfNoServiceProviderConfigured()
        {
            var mappingEx = Should.Throw<MappingException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.Before
                        .MappingBegins
                        .Call(ctx => ctx.GetService<Logger>().Log("Mapping!"));

                    var source = new PublicField<string> { Value = "Logging!" };

                    mapper.Map(source).Over(new PublicField<string>());
                }
            });

            mappingEx.InnerException.ShouldNotBeNull();

            // ReSharper disable once PossibleNullReferenceException
            mappingEx.InnerException.Message.ShouldContain("No service providers configured");
        }

        [Fact]
        public void ShouldErrorIfNoNamedServiceProviderConfigured()
        {
            var mappingEx = Should.Throw<MappingException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping.UseServiceProvider(t => new Logger());

                    mapper.Before
                        .MappingBegins
                        .Call(ctx => ctx.GetService<Logger>("SpecialLogger"));

                    var source = new PublicField<string> { Value = "Logging!" };

                    mapper.Map(source).Over(new PublicField<string>());
                }
            });

            mappingEx.InnerException.ShouldNotBeNull();

            // ReSharper disable once PossibleNullReferenceException
            mappingEx.InnerException.Message.ShouldContain("No named service providers configured");
        }

        [Fact]
        public void ShouldErrorIfDuplicateServiceProviderConfigured()
        {
            var mappingEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping.UseServiceProvider(t => new Logger());
                    mapper.WhenMapping.UseServiceProvider(t => new object());
                }
            });

            mappingEx.Message.ShouldContain("No named service providers configured");
        }

        [Fact]
        public void ShouldErrorWithServiceProviderInstanceTypeMismatch()
        {
            var mappingEx = Should.Throw<MappingException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping.UseServiceProvider(new GetServiceServiceProvider(new Logger()));

                    mapper.Before
                        .MappingBegins
                        .Call(ctx => ctx.GetServiceProvider<GetInstancesServiceProvider>());

                    var source = new PublicField<string> { Value = "Logging!" };

                    mapper.Map(source).Over(new PublicField<string>());
                }
            });

            mappingEx.InnerException.ShouldNotBeNull();

            // ReSharper disable once PossibleNullReferenceException
            mappingEx.InnerException.Message.ShouldContain("No service provider of type");
            mappingEx.InnerException.Message.ShouldContain("GetInstancesServiceProvider");
            mappingEx.InnerException.Message.ShouldContain("is configured");
        }

        [Fact]
        public void ShouldErrorIfNoServiceProviderMethodsAvailable()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
                Mapper.WhenMapping.UseServiceProvider(new Logger()));

            configEx.Message.ShouldContain("No supported service provider methods were found");
            configEx.Message.ShouldContain("Logger");
            configEx.Message.ShouldContain("GetService");
            configEx.Message.ShouldContain("GetInstance");
            configEx.Message.ShouldContain("Resolve");
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

        public class GetServiceServiceProvider
        {
            private readonly Logger _logger;

            public GetServiceServiceProvider(Logger logger)
            {
                _logger = logger;
            }

            public object GetService(Type serviceType) => _logger;
        }

        public class GetServiceWithNameServiceProvider
        {
            private readonly Logger _logger;
            private readonly string _name;

            public GetServiceWithNameServiceProvider(Logger logger, string name)
            {
                _logger = logger;
                _name = name;
            }

            public object GetService(Type serviceType, string name)
                => (name == _name) ? _logger : throw new NotSupportedException(_name);
        }

        public class GetInstancesServiceProvider
        {
            private readonly Logger _logger;
            private readonly string _name;

            public GetInstancesServiceProvider(Logger logger, string name = null)
            {
                _logger = logger;
                _name = name;
            }

            public object GetInstance(Type serviceType) => _logger;

            public object GetInstance(Type serviceType, string name)
                => (name == _name) ? _logger : throw new NotSupportedException(_name);
        }

        public class ResolveServiceProvider
        {
            private readonly Logger _logger;
            private readonly string _name;

            public ResolveServiceProvider(Logger logger, string name = null)
            {
                _logger = logger;
                _name = name;
            }

            public object GetInstance(Type serviceType) => _logger;

            public object GetInstance(Type serviceType, string name, string extraString = null, params object[] objects)
                => (name == _name) ? _logger : throw new NotSupportedException(_name);
        }

        #endregion
    }
}
