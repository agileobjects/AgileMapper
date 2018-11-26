By default, types are created using the 'greediest' public constructor - the one with the most parameters that have [matching](Member-Matching) source members. If there are no available constructors whose parameters can all be matched - and no parameterless constructor - the member for which the type would be created is ignored.

Constructor arguments can be configured by type or name, and constant values or expressions can be specified. 

For example, to configure mapping these types:

```C#
public class CustomerDto
{
    public string CustomerNum { get; set; }
    public string Name { get; set; }
}

public class Customer
{
    public Customer(Guid customerId, string customerName)
    {
    }
}
```

...use:

```C#
Mapper.WhenMapping
    .From<CustomerDto>()              // Apply to CustomerDto mappings
    .ToANew<Customer>()               // Apply to Customer creations
    .Map((dto, c) => dto.CustomerNum) // Map CustomerDto.CustomerNum
    .ToCtor<Guid>()                   // To Customer's Guid constructor param
    .And                              // Not done configuring yet...
    .Map((dto, c) => dto.Name)        // Map CustomerDto.Name
    .ToCtor("customerName");          // To Customer's 'customerName' param
```

...or, if inline configuration is preferred:

```C#
// Source, target and mapping types are implicit from the mapping:
Mapper
    .Map(customerDto).ToANew<Customer>(cfg => cfg
        .Map((dto, c) => dto.CustomerNum) // Map CustomerDto.CustomerNum
        .ToCtor<Guid>()                   // To Customer's Guid constructor param
        .And                              // Not done configuring yet...
        .Map((dto, c) => dto.Name)        // Map CustomerDto.Name
        .ToCtor("customerName"));         // To Customer's 'customerName' param
```

In these examples the `string` `CustomerNum` is parsed and [converted](Type-Conversion) to the `Guid` `customerId` out of the box.

If configuring constructor parameters is awkward (perhaps because there's a lot of them), you can also [configure an object factory](Configuring-Object-Creation) for a particular object type.