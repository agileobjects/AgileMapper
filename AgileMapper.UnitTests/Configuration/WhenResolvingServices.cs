namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using System.Collections.Generic;
    using AgileMapper.Configuration;
    using AgileMapper.Extensions;
    using Common;
    using Common.TestClasses;
#if NETCOREAPP
    using Microsoft.Extensions.DependencyInjection;
#endif
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    [Trait("Category", "Checked")]
    public class WhenResolvingServices
    {
        [Fact]
        public void ShouldUseAConfiguredServiceProvider()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var logger = new Logger();

                mapper.WhenMapping
                    .UseServiceProvider(_ => logger);

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
                        .UseServiceProvider((_, name) => (name == "Frank") ? logger : throw new NotSupportedException("NO!"));

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
        public void ShouldUseTheFirstMatchingServiceProviderMethodOnAnObject()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping.UseServiceProvider(new GetServiceOrInstanceServiceProvider());

                mapper.WhenMapping
                    .From<PublicField<string>>()
                    .Over<PublicField<string>>()
                    .Map(ctx => ctx.GetService<StringProvider>().Value)
                    .To(pf => pf.Value);

                var source = new PublicField<string> { Value = "Logging!" };
                var target = new PublicField<string> { Value = "Overwrite THIS" };

                mapper.Map(source).Over(target);

                target.Value.ShouldBe(nameof(StringProvider));
            }
        }

        [Fact]
        public void ShouldUseAServiceProviderInAnExceptionHandler()
        {
            var logger = new Logger();
            var provider = new ResolveServiceProvider(logger);

            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .UseServiceProvider(provider)
                    .PassExceptionsTo(exData => exData
                        .GetService<Logger>()
                        .Log(exData.Exception.ToString()));

                mapper.After
                    .CreatingInstances
                    .Call(_ => throw new InvalidOperationException("NO"));

                new PublicField<int> { Value = 123 }
                    .MapUsing(mapper)
                    .ToANew<PublicField<long>>();
            }

            logger.EntryCount.ShouldBe(1);
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

#if NETCOREAPP
        [Fact]
        public void ShouldUseAConfiguredServiceCollectionServiceProvider()
        {
            var collection = new ServiceCollection();

            collection.AddSingleton(new Logger());

            var provider = collection.BuildServiceProvider();

            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .UseServiceProvider(provider)
                    .PassExceptionsTo(exData => exData
                        .GetService<Logger>()
                        .Log(exData.Exception.ToString()));

                mapper.After
                    .CreatingInstances
                    .Call(ctx => throw new InvalidOperationException("NO"));

                new PublicField<int> { Value = 123 }
                    .MapUsing(mapper)
                    .ToANew<PublicField<long>>();
            }

            provider.GetService<Logger>().EntryCount.ShouldBe(1);
        }


        [Fact]
        public void ShouldExposeAConfiguredServiceCollectionServiceProvider()
        {
            var collection = new ServiceCollection();

            collection.AddSingleton<ILogger>(new Logger());

            var provider = collection.BuildServiceProvider();

            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping.UseServiceProvider(provider);

                mapper.Before
                    .MappingBegins
                    .Call(ctx =>
                    {
                        var log = ctx
                            .GetServiceProvider<IServiceProvider>()
                            .GetService<ILogger>();

                        log.Log("Logged!");
                    });

                var source = new PublicField<string> { Value = "Logging!" };

                mapper.Map(source).Over(new PublicField<string>());
            }

            provider.GetService<ILogger>().EntryCount.ShouldBe(1);
        }
#endif

        [Fact]
        public void ShouldErrorIfNullProviderSupplied()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping.UseServiceProvider(default(Func<Type, object>));
                }
            });

            configEx.InnerException.ShouldNotBeNull();
            configEx.InnerException.ShouldBeOfType<ArgumentNullException>();

            // ReSharper disable once PossibleNullReferenceException
            configEx.InnerException.Message.ShouldContain("serviceProvider");
        }

        [Fact]
        public void ShouldErrorIfNullNamedProviderSupplied()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping.UseServiceProvider(default(Func<Type, string, object>));
                }
            });

            configEx.InnerException.ShouldNotBeNull();
            configEx.InnerException.ShouldBeOfType<ArgumentNullException>();

            // ReSharper disable once PossibleNullReferenceException
            configEx.InnerException.Message.ShouldContain("serviceProvider");
        }

        [Fact]
        public void ShouldErrorIfNullProviderObjectSupplied()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping.UseServiceProvider(default(object));
                }
            });

            configEx.InnerException.ShouldNotBeNull();
            configEx.InnerException.ShouldBeOfType<ArgumentNullException>();

            // ReSharper disable once PossibleNullReferenceException
            configEx.InnerException.Message.ShouldContain("serviceProvider");
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
                    mapper.WhenMapping.UseServiceProvider(_ => new Logger());

                    mapper.Before
                        .MappingBegins
                        .Call(ctx => ctx.GetService<Logger>("SpecialLogger"));

                    var source = new PublicField<string> { Value = "Logging!" };

                    mapper.Map(source).Over(new PublicField<string>());
                }
            });

            mappingEx
                .InnerException.ShouldNotBeNull()
                .Message.ShouldContain("No named service providers configured");
        }

        [Fact]
        public void ShouldErrorIfDuplicateServiceProviderConfigured()
        {
            var mappingEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping.UseServiceProvider(_ => new Logger());
                    mapper.WhenMapping.UseServiceProvider(_ => new object());
                }
            });

            mappingEx.Message.ShouldContain("already been configured");
        }

        [Fact]
        public void ShouldErrorIfDuplicateNamedServiceProviderConfigured()
        {
            var mappingEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping.UseServiceProvider((_, _) => new Logger());
                    mapper.WhenMapping.UseServiceProvider((_, _) => new object());
                }
            });

            mappingEx.Message.ShouldContain("named");
            mappingEx.Message.ShouldContain("already been configured");
        }

        [Fact]
        public void ShouldErrorIfDuplicateServiceProviderObjectConfigured()
        {
            var mappingEx = Should.Throw<MappingConfigurationException>(() =>
            {
                var logger = new Logger();

                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping.UseServiceProvider(new GetServiceServiceProvider(logger));
                    mapper.WhenMapping.UseServiceProvider(new ResolveServiceProvider(logger));
                }
            });

            mappingEx.Message.ShouldContain("already been configured");
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

        [Fact]
        public void ShouldErrorIfServiceProviderMethodIsParameterless()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
                Mapper.WhenMapping.UseServiceProvider(new ParameterlessInvalidServiceProvider()));

            configEx.Message.ShouldContain("No supported service provider methods were found");
            configEx.Message.ShouldContain("ParameterlessInvalidServiceProvider");
        }

        [Fact]
        public void ShouldErrorIfServiceProviderMethodHasInvalidFirstParameterType()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
                Mapper.WhenMapping.UseServiceProvider(new InvalidFirstParameterTypeInvalidServiceProvider()));

            configEx.Message.ShouldContain("No supported service provider methods were found");
            configEx.Message.ShouldContain("InvalidFirstParameterTypeInvalidServiceProvider");
        }

        [Fact]
        public void ShouldErrorIfServiceProviderMethodHasInvalidSecondParameterType()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
                Mapper.WhenMapping.UseServiceProvider(new InvalidSecondParameterTypeInvalidServiceProvider()));

            configEx.Message.ShouldContain("No supported service provider methods were found");
            configEx.Message.ShouldContain("InvalidSecondParameterTypeInvalidServiceProvider");
        }

        [Fact]
        public void ShouldErrorIfServiceProviderMethodHasInvalidExtraParameterType()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
                Mapper.WhenMapping.UseServiceProvider(new InvalidExtraParameterTypeInvalidServiceProvider()));

            configEx.Message.ShouldContain("No supported service provider methods were found");
            configEx.Message.ShouldContain("InvalidExtraParameterTypeInvalidServiceProvider");
        }

        #region Helper Classes

        public interface ILogger
        {
            int EntryCount { get; }

            void Log(string message);
        }

        public class Logger : ILogger
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

        public class StringProvider
        {
            public string Value => nameof(StringProvider);
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

        public class GetServiceOrInstanceServiceProvider
        {
            public object GetService(Type serviceType) => Activator.CreateInstance(serviceType);

            public object GetInstance(Type serviceType) => Activator.CreateInstance(serviceType);
        }

        public class ParameterlessInvalidServiceProvider
        {
            public object GetService() => Activator.CreateInstance(typeof(object));
        }

        public class InvalidFirstParameterTypeInvalidServiceProvider
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            public object GetService(string serviceTypeName) => Activator.CreateInstance(Type.GetType(serviceTypeName));
        }

        public class InvalidSecondParameterTypeInvalidServiceProvider
        {
            public object GetService(Type serviceType, int ctorIndex) => Activator.CreateInstance(serviceType);
        }

        public class InvalidExtraParameterTypeInvalidServiceProvider
        {
            public object GetService(Type serviceType, string name, int ctorIndex) => Activator.CreateInstance(serviceType);
        }

        #endregion
    }
}
