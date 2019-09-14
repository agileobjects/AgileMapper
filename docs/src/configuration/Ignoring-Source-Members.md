[Compatible](/Type-Conversion) source members are automatically [matched](/Member-Matching) to target members, but you can tell a mapper to ignore source members which would usually be matched. For example:

```cs
public class OrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; }
}

public class Order
{
    public int? Id { get; set; }
    public string OrderNumber { get; set; }
}
```

`OrderDto.Id` member will be used to populate `Order.Id`, and `OrderDto.OrderNumber` will be used to populate `Order.OrderNumber`. To stop the `Id` mapping, use:

```cs
Mapper.WhenMapping
    .From<OrderDto>()             // Apply when mapping from OrderDto
    .Over<Order>()                // Apply the ignore to Order updates (optional)
    .IgnoreSource(dto => dto.Id); // Ignore the Order.Id property
```

Multiple members can be ignored with a single configuration, and ignores can be made conditional. Here's an [inline configuration](/configuration/Inline) example:

```cs
// Source, target and mapping types are implicit from the mapping:
Mapper.Map(orderDto).Over(order, cfg => cfg
    .If((dto, o) => dto.Id == 0) // Apply the ignores if OrderDto.Id is 0
    .IgnoreSource(
        dto => dto.Id,               // Ignore Order.Id...
        dto => dto.OrderNumber);     // ...and Order.OrderNumber
```

Source members can be ignored in several other ways, either globally (for all source and target types), or for specific source and target types.

## Source Member Filtering

Source members can be ignored by Type:

```cs
Mapper.WhenMapping
    .IgnoreSourceMembersOfType<IDontMapMe>(); // Global ignore

Mapper.WhenMapping
    .From<MySource>() // Apply when mapping from MySource
    .OnTo<MyTarget>() // Apply the ignore to MyTarget merges (optional)
    .IgnoreSourceMembersOfType<IDontMapMe>(); // Ignore all IDontMapMe members
```

...by member type:

```cs
Mapper.WhenMapping
    .IgnoreSourceMembersWhere(m => m.IsGetMethod); // Global ignore

Mapper.WhenMapping
    .ToANew<MyTarget>() // Apply the ignore to MyTarget creations
    .IgnoreSourceMembersWhere(m => m.IsField); // Ignore all fields

Mapper.WhenMapping
    .From<MySource>() // Apply when mapping from MySource (optional)
    .Over<MyTarget>() // Apply the ignore to MyTarget updates
    .IgnoreSourceMembersWhere(m => m.IsProperty); // Ignore all properties
```

...by member name:

```cs
Mapper.WhenMapping
    .IgnoreSourceMembersWhere(m => m.Name.Contains("NOPE")); // Global ignore

Mapper.WhenMapping
    .ToANew<MyTarget>() // Apply the ignore to MyTarget creations
    .IgnoreSourceMembersWhere(m => m.Name.Contains("NOPE")); // Ignore
```

...by Attribute:

```cs
Mapper.WhenMapping
    .IgnoreSourceMembersWhere(m => 
        m.HasAttribute<IgnoreDataMember>()); // Global ignore

Mapper.WhenMapping
    .OnTo<MyTarget>() // Apply the ignore to MyTarget merges
    .IgnoreSourceMembersWhere(m => 
        m.HasAttribute<IgnoreDataMember>()); // Ignore
```

...by member path:

```cs
Mapper.WhenMapping
    .IgnoreSourceMembersWhere(m => 
        m.Path.Contains("Customer.Address")); // Global ignore

Mapper.WhenMapping
    .Over<MyTarget>() // Apply the ignore to MyTarget updates
    .IgnoreSourceMembersWhere(m => 
        m.Path == "Customer.Address"); // Ignore
```

...or by `MemberInfo` matcher:

```cs
Mapper.WhenMapping
    .IgnoreSourceMembersWhere(m => 
        m.IsFieldMatching(f => f.IsAssembly)); // Global ignore

Mapper.WhenMapping
    .Over<MyTarget>() // Apply the ignore to MyTarget updates
    .IgnoreSourceMembersWhere(m => 
        m.IsPropertyMatching(p => p.IsAssembly)); // Ignore
```

Again, all ignores can alternatively be configured [inline](/configuration/Inline).

You can also ignore [target members](Ignoring-Target-Members)