[Query projection](/query-projection) supports a subset of regular mapping [configuration](/configuration). Anything [applicable](#limitations) configured using a `Mapper.WhenMapping` API - [static](/Static-vs-Instance-Mappers) or instance - will also be applied to query projections.

### Static API

For example:

```cs
// Configured on the default mapper, via the static API:
Mapper.WhenMapping
    .From<Product>()           // Apply to Product mappings
    .To<ProductDto>()          // Apply to all ProductDto mappings
    .Map((p, dto) => p.Spec)   // Map Product.Spec
    .To(p => p.Specification); // To ProductDto.Specification

// Using an EF Core DbContext;
// Product.Spec will be projected to ProductDto.Specification:
var productDtos = await context
    .Products
    .Project().To<ProductDto>()
    .ToArrayAsync();
```

In this case, the `Product.Spec` -> `ProductDto.Specification` custom [member value](/configuration/Member-Values) will be used in the projection because both the configuration and projection are performed using the default mapper.

### Instance API

You can achieve the same thing using an instance mapper by using `.ProjectUsing(mapper)` instead of `.Project()`. Any inline configuration you supply will be merged with the instance mapper's configuration in the usual way. For example:

```cs
// Configure an instance mapper via its API:
mapper.WhenMapping
    .From<Product>()           // Apply to Product mappings
    .ProjectedTo<ProductDto>() // Apply to all ProductDto projections
    .Map(p => p.Spec)          // Map Product.Spec
    .To(p => p.Specification); // To ProductDto.Specification

// Using an EF Core DbContext;
// Product.Spec will be projected to ProductDto.Specification, and 
// Product.Cost will be projected to ProductDto.Price conditionally:
var productDtos = await context
    .Products
    .ProjectUsing(mapper).To<ProductDto>(cfg => cfg
        .If(p => p.Cost > 0)
        .Map(p => p.Cost)
        .To(dto => dto.Price)
        .But
        .If(p => p.Cost == 0)
        .Map(p => p.Price)
        .To(dto => dto.Price))
    .ToArrayAsync();
```

### Limitations

Because projections are performed by [IQueryProvider](https://docs.microsoft.com/en-us/dotnet/api/system.linq.iqueryprovider)s, only the following configuration options are applied:

- Custom [member values](/configuration/Member-Values), as above

- Custom [constructor arguments](/configuration/Constructor-Arguments)

- Custom [naming patterns](/configuration/Member-Name-Patterns)

- To-String [formatting](/configuration/To-String-Formatting)

- Enum [pairing](/configuration/Enum-Mapping#configuring-enum-pairs)

- [Ignored](/configuration/Ignoring-Target-Members) members

- Object [factories](/configuration/Object-Construction)

- [Injected](/configuration/Dependency-Injection) values

- Null [mapping results](/configuration/Null-Results)

Most notably, [callbacks](/configuration/Mapping-Callbacks), [object tracking](/configuration/Mapped-Object-Tracking) and [derived type pairing](/configuration/Pairing-Derived-Types) are not supported - although you can [project to derived types](/query-projection/Derived-Types) conditionally.

In every case, it is up to you to configure values your IQueryProvider can support - configured values which will definitely be unsupported - like function invocations - are automatically filtered out of query projections.