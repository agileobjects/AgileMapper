# AgileMapper
AgileMapper is a zero-configuration, [highly-configurable](https://github.com/agileobjects/AgileMapper/wiki/Configuration) object-object mapper with [viewable execution plans](https://github.com/agileobjects/AgileMapper/wiki/Using-Execution-Plans) 
via a [static or instance](https://github.com/agileobjects/AgileMapper/wiki/Static-vs-Instance-Mappers) API. It conforms to [.NET Standard 1.0](https://docs.microsoft.com/en-us/dotnet/articles/standard/library).

You can use it to create new objects:

```C#
var customerDto = Mapper.Map(customer).ToANew<CustomerDto>();
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
