A service provider object or pair of functions can be configured to be available in mappings and configuration.

### Configuring A Service Provider

To configure a service provider, use:

```cs
// Use a given Type => object provider Func:
Mapper.WhenMapping.UseServiceProvider(serviceType => 
    CreateService(serviceType));

// Use a given (Type, string) => object provider Func:
Mapper.WhenMapping.UseServiceProvider((serviceType, name) => 
    CreateService(serviceType, name));

// Use a duck-typed provider:
Mapper.WhenMapping.UseServiceProvider(myDuckTypedProvider);
```

### Using a [Duck-Typed](https://en.wikipedia.org/wiki/Duck_typing) Provider

The following methods will be used on a configured service provider, if they are available:

 - `GetService(Type type)`
 - `GetService(Type type, string name)`
 - `GetInstance(Type type)`
 - `GetInstance(Type type, string name)`
 - `Resolve(Type type)`
 - `Resolve(Type type, string name)`

This covers (at least):

- [.NET Core's IServiceProvider](https://docs.microsoft.com/en-us/dotnet/api/system.iserviceprovider.getservice)
- [StructureMap](http://structuremap.github.io/quickstart) / [Lamar](https://jasperfx.github.io/lamar/documentation/ioc/resolving/get-a-service-by-plugintype)
- [Unity](https://github.com/unitycontainer/unity)

...but any container with methods matching these signatures will work.

Which methods the registered container supports dictates which service-resolution methods will be available during a mapping or configuration. If none of these methods are present, a `MappingConfigurationException` will be thrown.

### Accessing Services

#### During Mapping

Once a provider is configured, services can be accessed via the mapping context:

```cs
// Register a duck-typed provider:
Mapper.WhenMapping.UseServiceProvider(serviceProvider);

// Get the default ILogger from the provider and use it at the start
// of every mapping. Get the 'PostLogger' ILogger instance and use it
// at the end of every mapping:
Mapper.Before.MappingBegins
    .Call(ctx => ctx.GetService<ILogger>().Log("Mapping started"))
    .And
    .After.MappingEnds
    .Call(ctx => ctx.GetService<ILogger>("PostLogger").Log("Mapping complete"));
```

To retrieve the originally-configured provider, use `ctx.GetServiceProvider<TServiceProvider>()`. Service providers and services are available in configured [member values](/configuration/Member-Values), [Exception handlers](/configuration/Exception-Handling), [mapping callbacks](/configuration/Mapping-Callbacks) and custom [object factories](/configuration/Object-Construction).

#### During Configuration

To access services in a [MapperConfiguration](/configuration/Classes) instance, use:

```cs
public class MyMappingConfiguration : MapperConfiguration
{
    protected override void Configure()
    {
        // Get a reference to a configured .NET Core IServiceProvider:
        var provider = GetServiceProvider<IServiceProvider>();

        // Get a reference to a configured ILogger - this just passes 
        // the request to the container and returns the result:
        var logger = GetService<ILogger>();

        logger.LogDebug("Configuration complete");
    }
}
```

If a named service is requested (`.GetService<TService>("MyService")`) and:

- No named service provider function has been configured, and
- No configured service provider has a method with the `(Type, string) => object` signature

...a `MappingConfigurationException` will be thrown.

### Testing

To test a `MapperConfiguration` without a service provider instance, use:

```cs
using AgileObjects.AgileMapper.Testing;

var logger = new Mock<ILogger>().Object;
var stubServiceProvider = new StubServiceProvider(logger);

Mapper.WhenMapping
    .UseServiceProvider(stubServiceProvider )
    .UseConfigurations.From<MyMapperConfiguration>();

// Assert MyMapperConfiguration has been applied
```

The `StubServiceProvider` caches services in a `Dictionary{Type, object}` against their concrete and implemented interface Types; if more than one supplied object is of the same Type or implements the same interface, an Exception will be thrown.

`StubServiceProvider` supports `GetService<TService>()`, but not `GetService<TService>(instanceName)`.