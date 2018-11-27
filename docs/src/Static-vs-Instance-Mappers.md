AgileMapper provides the same API in both instance and static scope, so you can use whichever you prefer. The static API is a wrapper around a default instance-scoped mapper.

### Static API

Use the static API via static methods on the `Mapper` class, for example:

```cs
// Configuration - done once at start-up:
Mapper.WhenMapping
    .From<Order>()
    .To<OrderDto>()
    .Map(ctx => ctx.Source.OrderNumber)
    .To(dto => dto.OrderNo);

// Mapping - done whenever required:
var orderDto = Mapper.Map(order).ToANew<OrderDto>();
```

To re-run static Mapper configuration - for example in testing scenarios - use:

```cs
// Clears configuration and cached objects:
Mapper.ResetDefaultInstance();
``` 

Using the static API enables mapping in less flexible scenarios where you aren't using [dependency injection](https://en.wikipedia.org/wiki/Dependency_injection).

### Instance API

Use the instance API via instance methods on an [`IMapper`](https://github.com/agileobjects/AgileMapper/blob/master/AgileMapper/IMapper.cs) object, for example:

```cs
// Configuration - done once at start-up:
var mapper = Mapper.CreateNew();
mapper.WhenMapping
    .From<Order>()
    .To<OrderDto>()
    .Map(ctx => ctx.Source.OrderNumber)
    .To(dto => dto.OrderNo);

// Mapping - done whenever required
var orderDto = mapper.Map(order).ToANew<OrderDto>();
```

Using the instance API enables injection of a mapper via the `IMapper` interface, which emphasises object composition, declares the mapper as a dependency, and means you can swap in a mock mapper during testing if required. It also enables use of mappers with different configurations in different scenarios, for example, setting this up via a StructureMap registry:

```cs
public class OrderMapperRegistry : Registry
{
    public OrderMapperRegistry()
    {
        var mapperOne = Mapper.CreateNew();
        // Do mapperOne.WhenMapping configuration...

        // Register mapperOne as the instance to pass to OrderOneController:
        For<OrderOneController>().Use<OrderOneController>()
            .Ctor<IMapper>().Is(mapperOne);

        var mapperTwo = Mapper.CreateNew();
        // Do mapperTwo.WhenMapping configuration...

        // Register mapperTwo as the instance to pass to OrderTwoController:
        For<OrderTwoController>().Use<OrderTwoController>()
            .Ctor<IMapper>().Is(mapperTwo);
    }
}
```