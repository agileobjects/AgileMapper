Multiple, dedicated configuration classes can be created by deriving from the abstract `MapperConfiguration` base class. This enables configuration to be split up and placed nearer where mapping is performed (although [inline](/configuration/Inline) is nearer).

`MapperConfiguration` provides the same configuration API, and exposes services you inject.

### Configuration and Caching

Mapper configuration is set up by implementing the abstract `Configure` method:

```cs
public class ProductMappingConfiguration : MapperConfiguration
{
    protected override void Configure()
    {
        // Configure default Mapper ProductDto -> Productmapping:
        WhenMapping
            .From<ProductDto>()
            .To<Product>()
            .Map((dto, p) => dto.Spec)
            .To(p => p.Specification)
            .And
            .Ignore(p => p.Price, p => p.CatNum);

        // Cache all Product -> ProductDto mapping plans:
        GetPlansFor<Product>().To<ProductDto>();
    }
}
```

In this example the default mapper is configured - the one used via the [static](/Static-vs-Instance-Mappers) Mapper API.

### Applying Configurations

`MapperConfiguration` classes can be discovered and applied in various ways.

To apply a particular `MapperConfiguration` Type, supply it explicitly:

```cs
Mapper.WhenMapping
    .UseConfigurations.From<ProductMappingConfiguration>();
```

To apply all `MapperConfiguration` Types from an Assembly, supply a Type from that Assembly:

```cs
Mapper.WhenMapping
    .UseConfigurations.FromAssemblyOf<Product>();
```

To apply all `MapperConfiguration` Types from multiple Assemblies, supply the Assemblies:

```cs
// Scan all Assemblies from the AppDomain:
Mapper.WhenMapping
    .UseConfigurations.From(assembly1, assembly2, assembly3);

// Scan all the given Assemblies which match the filter -
// Assembly scanning can be expensive, so this can be useful!
Mapper.WhenMapping
    .UseConfigurations.From(
        GetLotsOfAssemblies(),
        assembly => assembly.FullName.Contains(nameof(MyNamespace)));
```

To apply all `MapperConfiguration` Types from the Assemblies current loaded into the `AppDomain`, use:

```cs
// Scan all Assemblies from the AppDomain:
Mapper.WhenMapping
    .UseConfigurations.FromCurrentAppDomain();

// Scan all Assemblies from the AppDomain which match the filter -
// Assembly scanning can be expensive, so this is advisable!
Mapper.WhenMapping
    .UseConfigurations.FromCurrentAppDomain(
        assembly => assembly.FullName.Contains("MyCompanyName"));
```

### Ordering Configurations

Calling `GetPlansFor<Source>().To<Target>()` caches the mapping function at the point you call it. If Types configured in the object graph are configured in more than one `MapperConfiguration`, you might need to define an order in which configuration classes are applied. Use:

```cs
// Configure aspects of Parent -> Parent mapping, which includes 
// mapping Child -> Child. Automatically apply ChildMapperConfiguration,
// then apply this configuration afterwards.
[ApplyAfter(typeof(ChildMapperConfiguration))]
public class ParentMapperConfiguration : MapperConfiguration
{
}

// Configure aspects of Child -> Child mapping:
public class ChildMapperConfiguration : MapperConfiguration
{
}
```

Chains of `ApplyAfter` attributes will be followed, with all configurations automatically applied in the defined order. Defining circular references between configuration types will throw a `MappingConfigurationException`. 

### Accessing Services

[Configured Service Providers](/configuration/Dependency-Injection) are available to `MapperConfiguration` classes. For example:

```cs
// Get a Dependency Injection container:
var diContainer = GetDiContainer();

// Register the container:
Mapper.WhenMapping.UseServiceProvider(diContainer);

// Scan for MapperConfigurations:
Mapper.WhenMapping
    .UseConfigurations.FromAssemblyOf<Product>();
```

...the DI container and its registered services are now available to the `MapperConfiguration` class via the `GetService<TService>()` and `GetServiceProvider<TContainer>()` methods:

```cs
public class MyMappingConfiguration : MapperConfiguration
{
    protected override void Configure()
    {
        // Get a reference to the configured container:
        var diContainer = GetServiceProvider<IUnityContainer>();

        // Get a reference to a configured ILogger - this just passes 
        // the request to the container and returns the result:
        var logger = GetService<ILogger>();

        // Create a new mapper for Product mapping:
        var productMapper = CreateNewMapper();

        // Configure productMapper Product -> ProductDto mapping:
        productMapper.WhenMapping
            .From<ProductDto>()
            .To<Product>()
            .Map((dto, p) => dto.Spec)
            .To(p => p.Specification);

        // Cache all Product -> ProductDto mapping plans:
        productMapper.GetPlansFor<Product>().To<ProductDto>();

        // Create a new mapper for Order mapping:
        var orderMapper = CreateNewMapper();

        // Configure orderMapper Order -> OrderDto create new mapping:
        orderMapper.WhenMapping
            .From<Order>()
            .ToANew<OrderDto>()
            .Map((o, dto) => o.Items.Sum(i => i.Cost))
            .To(dto => dto.TotalCost);

        // Cache the Order -> OrderDto create new mapping plan:
        orderMapper.GetPlanFor<Order>().ToANew<OrderDto>();

        // Register named IMapper instances with the container:
        diContainer.RegisterInstance("ProductMapper", productMapper);
        diContainer.RegisterInstance("OrderMapper", orderMapper);

        logger.LogDebug("Product and Order mapping configured and registered");
    }
}
```