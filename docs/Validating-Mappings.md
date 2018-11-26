A mapper's [execution plans](Using-Execution-Plans) can be validated to ensure that:

- All target members have [matching](Member-Matching) source values
- All target complex types can either be [constructed](Object-Construction), or have mappable target members
- All members of any source [enums](Enum-Mapping) being mapped to target enums have matching members in the target enum type

Mapping plan validation is intended to be used during development to make sure nothing is missed - you should remove it in production code.

### Validating inline

To validate that a mapping plan created on-the-fly is complete, use:

```C#
mapper
    .Map(customerDto).Over(customer, cfg => cfg
        .ThrowNowIfMappingPlanIsIncomplete());

// Or, with separate inline configuration:
mapper
    .Map(customerDto).Over(
        customer, 
        cfg => cfg.Map((d, c) => d.CustomerId).To(c => c.Id),
        cfg => cfg.ThrowNowIfMappingPlanIsIncomplete());
```

### Validating cached mapping plans

To validate all cached mapping plans, use:

```C#
// Cache the mapping plans you want to use later:
mapper.GetPlansFor<CustomerDto>().To<Customer>();
mapper.GetPlanFor<Customer>().ToANew<CustomerDto>();
mapper.GetPlanFor<Product>().ToANew<ProductViewModel>();
// etc.

// Validate that the cached plans are all complete:
mapper.ThrowNowIfAnyMappingPlanIsIncomplete();
```

### Validating by default

To configure a mapper to validate mapping plans by default, use:

```C#
Mapper.WhenMapping.ThrowIfAnyMappingPlanIsIncomplete();

// All throw if the plan is incomplete:
Mapper.GetPlanFor<Product>().ToANew<ProductDto>();

Mapper.Map(product).ToANew<ProductDto>();

Mapper.Map(product).ToANew<ProductDto>(cfg => cfg
    .Map((p, d) => p.Id).To(d => d.ProductId));
```