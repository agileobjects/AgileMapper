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

**A sequence of data sources**

```cs
// Map the Customer Home Address, Work Address
// then Address History to the CustomerViewModel
// AllAddresses property:
Mapper.WhenMapping
    .From<Customer>()                // Apply to Customer mappings
    .ToANew<CustomerViewModel>()     // Apply to CustomerViewModel creation
    .Map((c, vm) => new[] { c.HomeAddress })
        .Then.Map((c, vm) => new[] { c.WorkAddress })
        .Then.Map((c, vm) => c.AddressHistory)
    .To(vm => vm.AllAddresses);      // vm is the CustomerViewModel
```

### Conditional Data Sources

Any of these methods can be made conditional:

```cs
Mapper.WhenMapping
    .From<ProductDto>()                 // Apply to ProductDto mappings
    .ToANew<Product>()                  // Apply to Product creation only
    .If((dto, p) => dto.CompanyId == 0) // Apply only if CompanyId is 0
    .Map("No-one")                      // Always the same value
    .To(p => p.CompanyName);            // p is the Product
```

```cs
// Only include WorkAddress if it's different to HomeAddress:
Mapper.WhenMapping
    .From<Customer>()                // Apply to Customer mappings
    .ToANew<CustomerViewModel>()     // Apply to CustomerViewModel creation
    .Map((c, vm) => new[] { c.HomeAddress })
        .Then.If((c, vm) => c.WorkAddress != c.HomeAddress )
             .Map((c, vm) => new[] { c.WorkAddress })
        .Then.Map((c, vm) => c.AddressHistory)
    .To(vm => vm.AllAddresses);      // vm is the CustomerViewModel
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

### Mapping Nested Data Sources to the Target

To map a nested member to the target object, use, *e.g*:

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
class VideoDto
{
    public string Title { get; set; }
    public int LengthInSeconds { get; set; }
    public int ViewCount { get; set; }
}

Mapper.WhenMapping
    .From<Video>()      // Apply to Video mappings
    .To<VideoDto>()     // Apply to all VideoDto mappings
    .Map((v, dto) => v.Statistics)
    .ToTarget();        // The VideoDto is the target
```

In this example, the `ToTarget()` configuration causes `Video.Statistics.ViewCount` to be mapped to 
`VideoDto.ViewCount`. `Video.Title` is mapped to `VideoDto.Title` as expected.

### Switching Data Sources

To switch a mapping data source to a different value, use, _e.g_:

```csharp
class VideoLibrary
{
    public Dictionary<int, Video> VideosById { get; set; }
}

class VideoLibraryDto
{
    public IList<VideoDto> Videos { get; set; }
}

Mapper.WhenMapping
    .FromDictionariesWithValueType<Video>()
    .To<IList<VideoDto>>()
    .Map((d, l) => d.Values)
    .ToTargetInstead();
```
In this example, in any mapping where `Dictionary<string, Video>` is matched to an `IList<VideoDto>`, 
the Dictionary's `Values` collection is used as the source _instead_ of the Dictionary.

If a [conditional](#conditional-data-sources) `ToTargetInstead()`'s `If()` clause evaluates to true,
no further mapping is carried out for the member being mapped. If it evaluates to false, the default
mapping is performed, if any.

### Applying Data Sources with a Matcher

To apply a mapping data source to all target members matching particular criteria, use:

```csharp
// When mapping from bool -> string, map 'Y' or 'N' to any 
// members marked with a YesNoAttribute:
Mapper.WhenMapping
    .From<bool>() // Apply to bool mappings
    .To<string>() // Apply to all string mappings
    .IfTargetMemberMatches(m => m.HasAttribute<YesNoAttribute>())
    .Map((b, str) => b ? "Y" : "N") // Map 'Y' or 'N'
    .ToTarget();  // The bool is the target
```

`IfTargetMemberMatches()` data sources must be configured using `ToTarget()` or `ToTargetInstead()`;
as target member selection is performed using the matcher, it is invalid to specify a particular
target member, _e.g_ using `To(t => t.Name)`.

[Source](/configuration/Ignoring-Source-Members#source-member-filtering) and 
[target](/configuration/Ignoring-Target-Members#target-member-filtering) members can also be ignored
using a matcher.

#### Matching Options

Target members can be matched by type:

```csharp
// Match all string members:
Mapper.WhenMapping
    .To<ProductDto>() // Apply to all ProductDto mappings
    .IfTargetMemberMatches(m => m.HasType<string>())
    .Map("MatchedByType")
    .ToTarget();
```

...by member type:

```csharp
// Match fields and properties:
Mapper.WhenMapping
    .ToANew<ProductDto>() // Apply to ProductDto creations
    .IfTargetMemberMatches(m => m.IsField || m.IsProperty)
    .Map("MatchedByMatcher")
    .ToTarget();
```

...by member name:

```csharp
// Match any Product members with names starting with 'Id':
Mapper.WhenMapping
    .To<Product>() // Apply to all Product mappings
    .IfTargetMemberMatches(m => m.Name.StartsWith("Id"))
    .Map("MatchedByName")
    .ToTarget();
```

...by member path:

```csharp
Mapper.WhenMapping
    .To<Customer>() // Apply to all Customer mappings
    .IfTargetMemberMatches(m =>
        m.Path.StartsWith("ContactDetails.") &&
        m.Path.Contains("Address"))
    .Map("MatchedByPath")
    .ToTarget();
```

...or by `MemberInfo` matcher:

```csharp
Mapper.WhenMapping
    .To<Customer>() // Apply to all Customer mappings
    .IfTargetMemberMatches(m =>
        m.IsFieldMatching(f => f.IsSpecialName))
    .Map("MatchedByFieldMatcher")
    .ToTarget();
```





