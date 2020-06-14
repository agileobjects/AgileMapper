Custom factories can be configured to create instances of a type. Once created, the mapper will 
map its members as usual. To configure a factory to perform an entire mapping, use 
[MapInstancesUsing](/configuration/Object-Mapping).

Configure a custom factory for the creation of a particular type using:

```cs
Mapper.WhenMapping
    .InstancesOf<Customer>() // Apply to Customer creation, updates and merges
    .CreateUsing(ctx => new Customer
    {
        Number = ctx.EnumerableIndex
    });
```

Configure a custom factory for the creation of a particular type when mapping between particular
types using:

```cs
Mapper.WhenMapping
    .From<PersonViewModel>() // Apply to PersonViewModel mappings
    .To<Customer>()          // Apply to Customer creation, updates and merges
    .CreateInstancesUsing((pvm, c, i) => new Customer
    {
        Number = i
    });
```

Configure a conditional custom factory using ([inline](/configuration/Inline) example):

```cs
Mapper.Map(customerViewModels).ToANew<Customer[]>(cfg => cfg
    .WhenMapping
    .From<CustomerViewModel>()
    .To<Customer>()
    .If((cvm, c) => cvm.Discount > 0) // Apply if view model Discount > 0
    .CreateInstancesUsing((cvm, c, i) => new Customer
    {
        HasDiscount = true,
        Number = i
    }));
```