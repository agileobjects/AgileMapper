Custom formatting strings can be configured for given source types:

```C#
// Map DateTimes to string as '<month name> <year>'
Mapper.WhenMapping
    .StringsFrom<DateTime>(_ => _.UseFormat("y"));

// Map decimals to strings as currency (inline example):
Mapper
    .Map(productDto).ToANew<Product>(cfg => cfg
        .WhenMapping
        .StringsFrom<decimal>(_ => _.UseFormat("c")));
```

Formatting strings supplied using `Mapper.WhenMapping` will be applied to all mappings performed by the configured mapper. If an invalid formatting string is supplied an exception may occur during mapping.