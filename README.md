# AgileMapper

[![NuGet version](https://badge.fury.io/nu/AgileObjects.AgileMapper.svg)](https://badge.fury.io/nu/AgileObjects.AgileMapper)

AgileMapper is a zero-configuration, [highly-configurable](https://github.com/agileobjects/AgileMapper/wiki/Configuration) object-object mapper with [viewable execution plans](https://github.com/agileobjects/AgileMapper/wiki/Using-Execution-Plans). 
It projects queries, transforms, deep clones, updates and merges via extension methods, or a [static or instance](https://github.com/agileobjects/AgileMapper/wiki/Static-vs-Instance-Mappers) API. 
It targets [.NET Standard 1.0+](https://docs.microsoft.com/en-us/dotnet/articles/standard/library) and .NET 3.5+

You can use it to create new objects:

```C#
var customerDto = Mapper.Map(customer).ToANew<CustomerDto>();
```

...[project queries](https://github.com/agileobjects/AgileMapper/wiki/Query-Projection):

```C#
var customerDtos = await context
    .Customers
    .Project().To<CustomerDto>()
    .ToArrayAsync();
```

...perform [id-aware updates](https://github.com/agileobjects/AgileMapper/wiki/Performing-Updates):

```C#
Mapper.Map(customerViewModel).Over(customer);
```

...and [merges](https://github.com/agileobjects/AgileMapper/wiki/Performing-Merges):

```C#
Mapper.Map(customerOne).OnTo(customerTwo);
```

It's [available via NuGet](https://www.nuget.org/packages/AgileObjects.AgileMapper) and licensed with the 
[MIT licence](https://github.com/agileobjects/AgileMapper/blob/master/LICENCE.md). Check out [the wiki](https://github.com/agileobjects/AgileMapper/wiki)
for more information!
