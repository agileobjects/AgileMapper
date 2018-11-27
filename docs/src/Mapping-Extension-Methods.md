If you include:

```cs
using AgileObjects.AgileMapper.Extensions;
```

...mapping can be performed via extension methods. For example:

```cs
// Create a new CustomerViewModel from a Customer:
var customerViewModel = customer.Map().ToANew<CustomerViewModel>();

// Deep-clone a Customer:
var clonedCustomer = customerToBeCloned.DeepClone();

// Update a Customer from a CustomerSaveRequest
customerSaveRequest.Map().Over(customer);

// Merge a CustomerDto onto a Customer:
customerDto.Map().OnTo(customer);
```

### Using an Instance-Scoped Mapper

Mappings performed via these extension methods use the default mapper - the same one you map with via the [static Mapper API](/Static-vs-Instance-Mappers). To use an instance mapper with an extension method, use:

```cs
// Deep-clone a Customer using 
// the given instance-scoped mapper:
var clonedCustomer = customer
    .DeepCloneUsing(myInstanceScopedMapper);

// Update a Customer from a CustomerSaveRequest using
// the given instance-scoped mapper:
customerSaveRequest
    .MapUsing(myInstanceScopedMapper)
    .Over(customer);

// etc.
```

### Configuring Inline

You can also configure extension-method mappings inline, for example:

```cs
var customerSaveRequest = customerDto.Map().ToANew<CustomerSaveRequest>(cfg => cfg
    .Map((dto, csr) => dto.Id)
    .To(csr => csr.CustomerId));
```

...and combine that with an instance-scoped mapper, for example:

```cs
customerSaveRequest
    .MapUsing(instanceMapper)
    .Over(customer, cfg => cfg
        .Map((csr, c) => csr.AddressSaveRequest)
        .To(c => c.Address));
```