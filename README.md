# AgileMapper
AgileMapper is a zero-configuration, highly-configurable, portable object-object mapper with [viewable execution 
plans](https://github.com/agileobjects/AgileMapper/wiki/Using-Execution-Plans) via a static or instance API.

You can use it to create new objects:

```C#
var customerDto = Mapper.Map(customer).ToANew<CustomerDto>();
```

...perform id-aware updates:

```C#
Mapper.Map(customerViewModel).Over(customer);
```

...and merges:

```C#
Mapper.Map(customerOne).OnTo(customerTwo);
```

It's [available via NuGet](https://www.nuget.org/packages/AgileObjects.AgileMapper) and licensed with the 
[MIT licence](https://github.com/agileobjects/AgileMapper/blob/master/LICENCE.md). Check out [the wiki](https://github.com/agileobjects/AgileMapper/wiki)
for more information!