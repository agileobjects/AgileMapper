Configure a custom factory for a particular type using:

```C#
Mapper.WhenMapping
    .InstancesOf<Customer>() // Apply to Customer creation, updates and merges
    .CreateUsing(ctx => new Customer
    {
        Number = ctx.EnumerableIndex
    });
```

Configure a custom factory for a particular type when mapping between particular types using:

```C#
Mapper.WhenMapping
    .From<PersonViewModel>() // Apply to PersonViewModel mappings
    .To<Customer>()          // Apply to Customer creation, updates and merges
    .CreateInstancesUsing((pvm, c, i) => new Customer
    {
        Number = i
    });
```

Configure a conditional custom factory using ([inline](Inline-Configuration) example):

```C#
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