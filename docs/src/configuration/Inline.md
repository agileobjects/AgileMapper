To supply some or all of a configuration inline - at the point the mapping is performed - use:

```cs
var dto = mapper
    .Map(product).ToANew<ProductDto>(cfg => cfg
        .Map((p, d) => p.Spec).To(d => d.Specification));
```

This also works in [query projections](/query-projection), for example:

```cs
var dto = await context
    .Products
    .Project().To<ProductDto>(cfg => cfg
        .Map(p => p.Spec).To(d => d.Specification))
    .FirstOrDefaultAsync();
```

To supply multiple lines of configuration, use:

```cs
var dto = mapper
    .Map(product).ToANew<ProductDto>(cfg => cfg
        .Map((p, d) => p.Spec).To(d => d.Specification)
        .And
        .Map((p, d) => p.Price).To(d => d.Cost));

// Or if you prefer:
var dto = mapper
    .Map(product).ToANew<ProductDto>(
        cfg => cfg.Map((p, d) => p.Spec).To(d => d.Specification),
        cfg => cfg.Map((p, d) => p.Price).To(d => d.Cost));
```

To combine mapper configuration with inline configuration, use:

```cs
// Configuration in app startup code:
mapper.WhenMapping
    .From<Product>().To<ProductDto>()
    .Map((p, d) => p.Spec).To(d => d.Specification);

// Configuration at the point the mapping is performed:
var dto = mapper
    .Map(product).ToANew<ProductDto>(cfg => cfg
        .Map((p, d) => p.Price).To(d => d.Cost));
```

Inline configuration can be supplied via the [static](/Static-vs-Instance-Mappers) or [instance](/Static-vs-Instance-Mappers) APIs.

### Invalid Configuration

If inline configuration is invalid, a `MappingConfigurationException` will be thrown when the mapping is attempted. For example:

```cs
// Supply two different sources for ProductDto.Specification;
// throws an Exception!
var dto = mapper
    .Map(product).ToANew<ProductDto>(cfg => cfg
        .Map((p, d) => p.Spec).To(d => d.Specification)
        .And
        .Map((p, d) => p.Price).To(d => d.Specification));
````

As usual, code which uses mapping should be covered by tests to prevent errors.

### Side Effects

The first time an inline-configured mapping is performed, the mapper's configuration is cloned and combined with the the configuration you supply. The combined configuration is then cached.

Finding the combined configuration for subsequent mappings incurs a [very] small performance penalty. For example, in the following mapping:

```cs
public class Product
{
    public string Spec { get; set; }
    public double Price { get; set; }
}

public class ProductDto
{
    public string Specification { get; set; }
    public double Cost { get; set; }
}

var dto = mapper
    .Map(new Product { Spec = "This is the spec!", Price = 99.99 })
    .ToANew<ProductDto>(cfg => cfg
        .Map((p, d) => p.Spec).To(d => d.Specification)
        .And
        .Map((p, d) => p.Price).To(d => d.Cost))
```

...examining the supplied specification and retrieving the cached configuration takes approximately 0.02 milliseconds. Over the course of 1 million mappings (*ie*: 1 million calls to `.Map()`, not one call to `.Map()` with 1 million objects), that adds up to a total delay of 20 seconds. The more extensive the configuration, the longer the delay. This should be considered if mapping performance is a priority concern, and testing the performance difference is recommended in that case.