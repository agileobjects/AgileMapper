# AgileMapper

[![NuGet version](https://badge.fury.io/nu/AgileObjects.AgileMapper.svg)](https://badge.fury.io/nu/AgileObjects.AgileMapper)

AgileMapper is a zero-configuration, [highly-configurable](https://agilemapper.readthedocs.io/configuration), 
unopinionated object mapper with execution plans you can [view](https://agilemapper.readthedocs.io/Using-Execution-Plans),
or [generate and build](https://agilemapper.readthedocs.io/mapper-generation) into your source code. 
It flattens, unflattens, deep clones, [merges](https://agilemapper.readthedocs.io/Performing-Merges), 
[updates](https://agilemapper.readthedocs.io/Performing-Updates) and [projects queries](https://agilemapper.readthedocs.io/query-projection)
via [extension methods](https://agilemapper.readthedocs.io/Mapping-Extension-Methods), or a 
[static or instance](https://agilemapper.readthedocs.io/Static-vs-Instance-Mappers) API. 
It targets .NET 3.5+ and [.NET Standard 1.0+](https://docs.microsoft.com/en-us/dotnet/articles/standard/library).

You can use it to create new objects:

```C#
var customerDto = Mapper.Map(customer).ToANew<CustomerDto>();
```

...[project queries](https://agilemapper.readthedocs.io/query-projection):

```C#
var customerDtos = await context
    .Customers
    .Project().To<CustomerDto>()
    .ToArrayAsync();
```

...perform [id-aware updates](https://agilemapper.readthedocs.io/Performing-Updates):

```C#
Mapper.Map(customerViewModel).Over(customer);
```

...and [merges](https://agilemapper.readthedocs.io/Performing-Merges):

```C#
Mapper.Map(customerOne).OnTo(customerTwo);
```

It's [available via NuGet](https://www.nuget.org/packages/AgileObjects.AgileMapper), with code 
generation performed by [this extension](https://www.nuget.org/packages/AgileObjects.AgileMapper.Buildable),
both licensed with the [MIT licence](https://github.com/agileobjects/AgileMapper/blob/master/LICENCE.md).
Check out [the documentation](https://agilemapper.readthedocs.io) for more!
