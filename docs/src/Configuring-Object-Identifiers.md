Objects are identified during collection [updates](Performing-Updates) and [merges](Performing-Merges) using members with the following names:

* `Id`
* `<Type name>Id`
* `Identifier`
* `<Type name>Identifier`

You can configure an object to be uniquely identified with a different member using:

```cs
Mapper.WhenMapping
    .InstancesOf<NamedCustomer>() // Apply to NamedCustomer mappings
    .IdentifyUsing(c => c.Name);  // Identify using NamedCustomer.Name
```

Or, configured [inline](Inline-Configuration):

```cs
Mapper
    .Map(customerDtos).ToANew<NamedCustomer[]>(cfg => cfg
        .WhenMapping
        .InstancesOf<NamedCustomer>()
        .IdentifyUsing(c => c.Name)); // Identify using NamedCustomer.Name
```