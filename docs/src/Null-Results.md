If a target complex type member has no [matching source member](Member-Matching), no matched [constructor arguments](Object-Construction), and none of its child members have matching source members, it will be mapped to null.

For example:

```cs
var source = new { Name = "Frank" };
var target = new Person { Name = "Charlie", Address = default(Address) };
Mapper.Map(source).Over(target);

// target.Address will be null
```

To configure a condition under which a complex type mapping should return null instead of an instantiated object, use (e.g.):

```cs
Mapper.WhenMapping
    .ToANew<Address>()
    .If((o, a) => 
        string.IsNullOrWhiteSpace(a.Line1) || 
        string.IsNullOrWhiteSpace(a.Postcode))
    .MapToNull();
```

This only applies to complex type properties (e.g. Person.Address), not collection elements. Collection elements are mapped to instantiated objects rather than being left with null entries. 