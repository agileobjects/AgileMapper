If a naming convention prevents normal [member matching](/Member-Matching), you can configure naming patterns to use to match names up. For example:

```cs
public class ProductDto
{
    public string strName { get; set; }
    public double decPrice { get; set; }
}
public class Product
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

To have the name prefixes `str` and `dec` ignored when matching member names, use:

```cs
Mapper.WhenMapping
    .UseNamePrefixes("str", "dec"); 
```

You can also configure name suffixes:

```cs
Mapper.WhenMapping
    .UseNameSuffix("Value"); // Match 'PriceValue' to 'Price'
```

...or a regex pattern:

```cs
Mapper.WhenMapping
    .UseNamePattern("^_(.+)Value$"); // Match '_PriceValue' to 'Price'
```

Configured regex patterns must start with `^` and end with `$`, contain the capturing group `(.+)` to provide the part of a name to use for matching, and have a prefix and / or suffix.

Naming patterns can also be configured [inline](/configuration/Inline)

```cs
var anonSource = new { _PriceValue = default(double) };

// Source, target and mapping types are implicit from the mapping:
Mapper
    .Map(anonSource).ToANew<Product>(cfg => cfg
        .UseNamePattern("^_(.+)Value$")); // Match '_PriceValue' to 'Price'
```

...or for specific source and target types:

```cs
var anonSource = new { _PriceValue = default(double) };

Mapper.WhenMapping
    .From(anonSource)  // Apply to this anon type's mappings
    .To<Product>()     // Apply to Product mappings
    .UseNamePattern("^_(.+)Value$"); // Match '_PriceValue' to 'Price'
```