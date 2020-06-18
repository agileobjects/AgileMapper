Custom factories can be configured to map instances of a type. These factories perform the entire
mapping - the mapper executes them and does not further mapping for that object. To configure a 
factory to create an object and have the mapper map its members, use 
[CreateInstancesUsing](/configuration/Object-Construction).

Configure a custom factory for the mapping of a particular type using:

```cs
Mapper.WhenMapping
    .From<CustomerViewModel>() // Apply to CustomerViewModel mappings
    .ToANew<Customer>()        // Apply to Customer creations
    .MapInstancesUsing(ctx => new Customer
    {
        Name = ctx.Source.Forename + " " + ctx.Source.Surname,
        Number = ctx.Source.CustomerNo
    });
```

Configure a conditional custom factory using ([inline](/configuration/Inline) example):

```cs
Mapper.Map(customerViewModel).ToANew<Customer>(cfg => cfg
    .If((cvm, c) => cvm.Discount > 0) // Apply if view model Discount > 0
    .MapInstancesUsing(ctx => new Customer
    {
        Name = ctx.Source.Forename + " " + ctx.Source.Surname,
        Number = ctx.ElementIndex,
        HasDiscount = true
    }));
```