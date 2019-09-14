[Compatible](/Type-Conversion) source members are automatically [matched](/Member-Matching) to target members, but you can tell a mapper to ignore source values if they match a given condition. For example:

```cs
public class OrderDto
{
    public string Id { get; set; }
}

public class Order
{
    public string Id { get; set; }
}
```

When updating an `Order` from an `OrderDto`, `Order.Id` is overwritten with the value of `OrderDto.Id` - including if `OrderDto.Id` is null or an empty string. You can stop this using:

```cs
Mapper.WhenMapping
    .Over<Order>()                                   // Apply the filter to Order updates
    .IgnoreSources(c => c.If(string.IsNullOrEmpty)); // Ignore null or empty source strings
```

Multiple value types can be filtered with a single configuration, and filters can be made conditional. Here's an [inline configuration](/configuration/Inline) example:

```cs
// Source, target and mapping types are implicit from the mapping:
Mapper.Map(orderDto).Over(order, cfg => cfg
    .If((dto, o) => dto.Id == "0")                 // Apply the filters only if OrderDto.Id is 0
    .IgnoreSources(c => c
        .If(o => o == null) ||                     // Ignore null source values of any type
        .If<string>(str => str == string.Empty) || // Ignore empty source strings
        .If<int>(i => i == int.MinValue));         // Ignore int.MinValue source ints
```

Source value filters can also be configured globally:

```cs
// Ignore null or whitespace source strings in mappings
// for all rule sets (create new, update, merge, etc) and
// for all source and target types:
Mapper.WhenMapping
    .IgnoreSources(c => c.If(string.IsNullOrWhiteSpace))
```