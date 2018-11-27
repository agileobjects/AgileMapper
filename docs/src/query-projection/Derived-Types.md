If your [`IQueryProvider`](https://docs.microsoft.com/en-us/dotnet/api/system.linq.iqueryprovider) [supports](/query-projection/Entity-Framework#derived-types) casting projected instances to a base Type, you can project to derived Types via configured conditions. For example:

```cs
// Using an EF Core DbContext:
var animalDtos = await context
    .Animals
    .ProjectTo<AnimalDto>(cfg => cfg
        .If(a => a.Type == AnimalType.Dog)
        .MapTo<DogDto>()
        .And
        .If(a => a.Type == AnimalType.Cat)
        .MapTo<CatDto>()
        .And
        .If(a => a.Type == AnimalType.Gorilla)
        .MapTo<GorillaDto>())
    .ToArrayAsync();
```

A query projection will be generated which creates the appropriate derived Type for the `AnimalType`. If the `Animal.Type` does not match a configured value, an `AnimalDto` is created. If `AnimalDto` is abstract, the `Animal` is projected to null.