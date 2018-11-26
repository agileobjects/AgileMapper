AgileMapper can project an `IQueryable{T}` to any other type supported by the [`IQueryProvider`](https://docs.microsoft.com/en-us/dotnet/api/system.linq.iqueryprovider), which can increase performance by only selecting the required data from an underlying data store, and decrease lines of code by directly materializing a DTO or view model, instead of materializing then mapping entities.

Query projection supports a subset of regular mapping [configuration](Query-Projection-Configuration), and can be configured [inline](Inline-Configuration). It also supports projection of recursive relationships to a specified depth.

To project a query, use:

```C#
// Using an EF Core DbContext:
var orderDtos = await context
    .Orders
    .Project().To<OrderDto>()
    .ToListAsync();
```

`.Project()` uses the default mapper - the one used behind the scenes by the [static](Static-vs-Instance-Mappers) `Mapper` API. To project a query using an instance-scoped mapper, use:

```C#
var orderDtos = await context
    .Orders
    .ProjectUsing(mapper).To<OrderDto>()
    .ToListAsync();
```

### Viewing Projection Plans

The [plan](Using-Execution-Plans) for a query projection can be viewed and cached just like those for regular mappings, but because they're cached against the IQueryProvider's Type, an instance of the queryable being projected is required.

For example:

```C#
// Cache the plan to project a Person to a PersonDto;
Mapper.GetPlanForProjecting(context.People).To<PersonDto>();
```