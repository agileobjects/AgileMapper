You can configure code to be executed at specific points in a mapping - conditionally if required. For example:

Before mapping of a specified type begins:

```cs
mapper.WhenMapping
    .To<Customer>() // Apply to Customer creation, updates and merges
    .Before
    .MappingBegins
    .Call(ctx => Console.WriteLine(
        "Mapping customer " + ctx.Target.CustomerId));
```

Before an instance of a specified type is created:

```cs
mapper.WhenMapping
    .From<AddressDto>() // Apply to AddressDto mappings
    .ToANew<Address>()  // Apply to Address creation
    .Before
    .CreatingTargetInstances
    .Call((dto, a) => dto.Line1 = dto.HouseNum + " " + dto.Line1);
```

After an instance of a specified type is created:

```cs
mapper
    .After
    .CreatingInstancesOf<Address>()
    .Call((o, a) => Console.WriteLine(
        $"Created Address {a.AddressId} from {o.GetType()}"));
```

On either side of the mapping of a particular member:

```cs
mapper.WhenMapping
    .From<Person>()   // Apply to Person mappings
    .Over<PersonVm>() // Apply to PersonVm updates
    .Before
    .Mapping(pvm => pvm.Name)
    .Call((p, pvm) => p.Name = p.Name ?? string.Empty)
    .And
    .After
    .Mapping(pvm => pvm.Name)
    .Call((p, pvm) => pvm.Name = p.Title + " " + pvm.Name);
```

At the end of any mapping (conditionally):

```cs
Mapper
    .After
    .MappingEnds
    .If(ctx => Debug.Listeners.Count > 0)
    .Call((s, t) => Debug.Print(
        $"Mapped a {s.GetType()} to a {t.GetType()}"));
```