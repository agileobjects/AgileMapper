# AgileMapper
AgileMapper is a zero-configuration, highly-configurable, portable object-object mapper with viewable execution 
plans via a static or instance API.

You can use it to deep-clone:

    var clonedCustomer = Mapper.Clone(customerToBeCloned);

...update:

    Mapper.Map(customerViewModel).Over(customer);

...and merge:

    Mapper.Map(customerOne).OnTo(customerTwo);

It's [available via NuGet](https://www.nuget.org/packages/AgileObjects.AgileMapper) and licensed with the 
[MIT licence](https://github.com/agileobjects/AgileMapper/blob/master/LICENCE.md). Check out [the wiki](https://github.com/agileobjects/AgileMapper/wiki)
for more information!