There are several ways of configuring a custom data source for a target member.

**A source member**:

```cs
Mapper.WhenMapping
    .From<ProductDto>()          // Apply to ProductDto mappings
    .To<Product>()               // Apply to Product creation, updates and merges
    .Map(ctx => ctx.Source.Spec) // ctx.Source is the ProductDto
    .To(p => p.Specification);   // p is the Product

// Or, more tersely:
Mapper.WhenMapping
    .From<ProductDto>().To<Product>()
    .Map(dto => dto.Spec, p => p.Specification);

```

**A source expression** (in this example, supplied [inline](/configuration/Inline)):

```cs
// Source, target and mapping types are implicit from the mapping:
Mapper.Map(productDto).ToANew<Product>(cfg => cfg
    .Map(ctx => "$" + ctx.Source.Price) // ctx.Source is the ProductDto
    .To(p => p.Price));                 // p is the Product    
```

**A constant value**:

```cs
Mapper.WhenMapping
    .From<ProductDto>()      // Apply to ProductDto mappings
    .OnTo<Product>()         // Apply to Product merges only
    .Map("Company ABC")      // Always the same value
    .To(p => p.CompanyName); // p is the Product

// Or, more tersely:
Mapper.WhenMapping
    .From<ProductDto>().OnTo<Product>()
    .Map("Company ABC", p => p.CompanyName);
```

**A value from an [injected service](/configuration/Dependency-Injection)**:

```cs
// Retrieve an IDateTimeProvider instance from a configured
// service provider, and use its UtcNow value:
Mapper.WhenMapping
    .From<OrderDto>()        // Apply to OrderDto mappings
    .ToANew<Order>()         // Apply to Order creation
    .Map(ctx => ctx.GetService<IDateTimeProvider>().UtcNow)
    .To(o => o.DateCreated); // o is the Order
```

**The result of a function call** ([inline](/configuration/Inline)):

```cs
Func<ProductDto, Product, string> companyNameFactory = 
    (dto, p) => dto.ManufaturerName + " Ltd";

// Source, target and mapping types are implicit from the mapping:
Mapper.Map(productDto).ToANew<Product>(cfg => cfg
    .Map(companyNameFactory)  // Map the factory function result
    .To(p => p.CompanyName)); // p is the Product
```

### Making Data Sources Conditional:

Any of these methods can be configured to be conditional:

```cs
Mapper.WhenMapping
    .From<ProductDto>()                 // Apply to ProductDto mappings
    .ToANew<Product>()                  // Apply to Product creation only
    .If((dto, p) => dto.CompanyId == 0) // Apply only if CompanyId is 0
    .Map("No-one")                      // Always the same value
    .To(p => p.CompanyName);            // p is the Product
```

And in an [inline](/configuration/Inline) example:

```cs
Mapper.Map(productDto).ToANew<Product>(cfg => cfg
    .If((dto, p) => dto.CompanyId == 0) // Apply only if CompanyId is 0
    .Map("No-one")                      // Always the same value
    .To(p => p.CompanyName);            // p is the Product
```

### Auto-Reversing Configured Data Sources

By default, configured data sources only apply in the direction configured - configuring `ProductDto.Spec` -> `Product.Specification` doesn't make `Product.Specification` map to `ProductDto.Spec` when the reverse mapping is performed.

To make every source- to target-member pairing you configure apply to mappings in either direction, use:

```cs
Mapper.WhenMapping.AutoReverseConfiguredDataSources();
```

To make every source- to target-member pairing you configure for a particular pair of Types apply to mappings in either direction, use:

```cs
Mapper.WhenMapping
    .From<Product>().To<ProductDto>()
    .AutoReverseConfiguredDataSources();
```

To make a source- to target-member pairing you configure apply to mappings in either direction, use:

```cs
Mapper.WhenMapping
    .From<Product>().To<ProductDto>()
    .Map(p => p.Specification, dto => dtp.Spec)
    .AndViceVersa();
```

#### Opting Out of Auto-Reversal

If you use the mapper-level `AutoReverseConfiguredDataSources()` to set the default behaviour, source- to target-member pairings you configure for a particular pair of Types can opt out using:

```cs
// Set the default behaviour:
Mapper.WhenMapping.AutoReverseConfiguredDataSources();

// Opt out for ProductDto -> Product:
Mapper.WhenMapping
    .From<Product>().To<ProductDto>()
    .DoNotAutoReverseConfiguredDataSources();
```

...and individual source- to target-member pairings can opt out with:

```cs
Mapper.WhenMapping
    .From<Product>().To<ProductDto>()
    .Map(p => p.Specification, dto => dtp.Spec)
    .ButNotViceVersa();
```

### Mapping Data Sources to the Root Target

To map a data source to the root target object, use, *e.g*:

```cs
// Source class - has a nested member 'Statistics':
class Video
{
    public string Title { get; set; }
    public int LengthInSeconds { get; set; }
    public VideoStatistics Statistics { get; set; }
}
class VideoStatistics
{
    public int ViewCount { get; set; }
}

// Target class:
public class VideoDto
{
    public string Title { get; set; }
    public int LengthInSeconds { get; set; }
    public int ViewCount { get; set; }
}

Mapper.WhenMapping
    .From<Video>()
    .To<VideoDto>()
    .Map((v, dto) => v.Statistics)
    .ToTarget();
```

In this example, the `ToTarget()` configuration causes `VideoDto.ViewCount` to be mapped from `Video.Statistics.ViewCount`.