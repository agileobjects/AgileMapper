Target members which have no [matching](Member-Matching), [compatible](Type-Conversion) source member are ignored by default, but you can also tell a mapper to ignore members which would usually be mapped. For example:

```cs
public class OrderDto
{
    public int Id { get; set; }
}

public class Order
{
    public int? Id { get; set; }
    public DateTime DateCreated { get; set; }
}
```

`Order.DateCreated` will be ignored because `OrderDto` has no matching member, but out of the box the `Id` property will be updated. You can stop this using:

```cs
Mapper.WhenMapping
    .From<OrderDto>()   // Apply when mapping from OrderDto (optional)
    .To<Order>()        // Apply the ignore to Order creation, updates and merges
    .Ignore(o => o.Id); // Ignore the Order.Id property
```

Multiple fields can be ignored with a single configuration, and ignores can be made conditional. Here's an [inline configuration](Inline-Configuration) example:

```cs
// Source, target and mapping types are implicit from the mapping:
Mapper
    .Map(orderDto).Over(order, cfg => cfg
        .If((dto, o) => dto.Id == 0) // Apply the ignores if OrderDto.Id is 0
        .Ignore(
            o => o.Id,
            o => o.DateCreated);     // Ignore Order.Id and Order.DateCreated
```

Target members can be ignored in several other ways, either globally (for all source and target types), or for specific source and target types.

## Member Filtering

You can ignore members by Type:

```cs
Mapper.WhenMapping
    .IgnoreTargetMembersOfType<IDontMapMe>(); // Global ignore

Mapper.WhenMapping
    .From<MySource>() // Apply when mapping from MySource (optional)
    .OnTo<MyTarget>() // Apply the ignore to MyTarget merges
    .IgnoreTargetMembersOfType<IDontMapMe>(); // Ignore all IDontMapMe members
```

...by member type:

```cs
Mapper.WhenMapping
    .IgnoreTargetMembersWhere(m => m.IsSetMethod); // Global ignore

Mapper.WhenMapping
    .ToANew<MyTarget>() // Apply the ignore to MyTarget creations
    .IgnoreTargetMembersWhere(m => m.IsField); // Ignore all fields

Mapper.WhenMapping
    .From<MySource>() // Apply when mapping from MySource (optional)
    .Over<MyTarget>() // Apply the ignore to MyTarget updates
    .IgnoreTargetMembersWhere(m => m.IsProperty); // Ignore all properties
```

...by member name:

```cs
Mapper.WhenMapping
    .IgnoreTargetMembersWhere(m => m.Name.Contains("NOPE")); // Global ignore

Mapper.WhenMapping
    .ToANew<MyTarget>() // Apply the ignore to MyTarget creations
    .IgnoreTargetMembersWhere(m => m.Name.Contains("NOPE")); // Ignore
```

...by Attribute:

```cs
Mapper.WhenMapping
    .IgnoreTargetMembersWhere(m => 
        m.HasAttribute<IgnoreDataMember>()); // Global ignore

Mapper.WhenMapping
    .OnTo<MyTarget>() // Apply the ignore to MyTarget merges
    .IgnoreTargetMembersWhere(m => 
        m.HasAttribute<IgnoreDataMember>()); // Ignore
```

...by member path:

```cs
Mapper.WhenMapping
    .IgnoreTargetMembersWhere(m => 
        m.Path.Contains("Customer.Address")); // Global ignore

Mapper.WhenMapping
    .Over<MyTarget>() // Apply the ignore to MyTarget updates
    .IgnoreTargetMembersWhere(m => 
        m.Path == "Customer.Address"); // Ignore
```

...or by `MemberInfo` matcher:

```cs
Mapper.WhenMapping
    .IgnoreTargetMembersWhere(m => 
        m.IsFieldMatching(f => f.IsAssembly)); // Global ignore

Mapper.WhenMapping
    .Over<MyTarget>() // Apply the ignore to MyTarget updates
    .IgnoreTargetMembersWhere(m => 
        m.IsPropertyMatching(p => p.IsAssembly)); // Ignore
```

Again, all ignores can alternatively be configured [inline](Inline-Configuration).