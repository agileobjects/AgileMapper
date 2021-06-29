## Overview

AgileMapper is a zero-configuration, [highly-configurable](/configuration), unopinionated object-object mapper with [viewable execution plans](/Using-Execution-Plans), targeting .NET 3.5+ and [.NET Standard 1.0+](https://docs.microsoft.com/en-us/dotnet/articles/standard/library). It flattens, unflattens, deep clones, [merges](/Performing-Merges), [updates](/Performing-Updates) and [projects queries](/query-projection) via [extension methods](/Mapping-Extension-Methods), or a [static or instance](/Static-vs-Instance-Mappers) API.

Mapping functions are created and cached the first time two types are mapped - no up-front configuration is necessary. You can [cache up-front](/Using-Execution-Plans) if you prefer, though.

[Available via NuGet](https://www.nuget.org/packages/AgileObjects.AgileMapper) and licensed with the [MIT licence](https://github.com/agileobjects/AgileMapper/blob/master/LICENCE.md), you can install it via the [package manager console](https://docs.nuget.org/consume/package-manager-console):

```shell
PM> Install-Package AgileObjects.AgileMapper
```

[![NuGet version](https://badge.fury.io/nu/AgileObjects.AgileMapper.svg)](https://badge.fury.io/nu/AgileObjects.AgileMapper)

## Basic Use

### Object Creation

Create an object from another using:

```cs
var customer = Mapper.Map(customerViewModel).ToANew<Customer>();
// Or:
var customer = customerViewModel.Map().ToANew<Customer>();
```

### Query Projection

[Project](/query-projection) entities to another Type using:

```cs
var customerVm = await dbContext
    .Customers
    .Project().To<CustomerViewModel>()
    .FirstAsync(c => c.Id == customerId);
```

### Deep Cloning

Deep-clone an object using:

```cs
var clonedCustomer = Mapper.DeepClone(customerToBeCloned);
// Or:
var clonedCustomer = customerToBeCloned.DeepClone();
```

### Updating

[Update](/Performing-Updates) an object's members with values from another using:

```cs
Mapper.Map(customerSaveRequest).Over(customer);
// Or:
customerSaveRequest.Map().Over(customer);
```

### Merging

[Merge](/Performing-Merges) an object's unpopulated members with values from another using:

```cs
Mapper.Map(customerDto).OnTo(customer);
// Or:
customerDto.Map().OnTo(customer);
```

## View a Mapping Execution Plan

View an [execution plan](/Using-Execution-Plans) to see how two object types will be mapped; this also caches the plan, so you can use it to choose when to incur that cost. Use:

```cs
// For object creation:
string mappingPlan = Mapper.GetPlanFor<Customer>().ToANew<CustomerViewModel>();
// For updates:
string mappingPlan = Mapper.GetPlanFor<Customer>().Over<CustomerViewModel>();
// For merges:
string mappingPlan = Mapper.GetPlanFor<Customer>().OnTo<CustomerViewModel>();

// For all three in one call:
string mappingPlans = Mapper.GetPlansFor<Customer>().To<CustomerViewModel>();

// For query projection:
string mappingPlan = Mapper
    .GetPlanForProjecting(context.Customers)
    .To<CustomerViewModel>();

// For everything in the mapping cache:
var allPlans = Mapper.GetPlansInCache();
```