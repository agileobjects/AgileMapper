By default, an `Exception` thrown during a mapping is wrapped in a [`MappingException`](/agileobjects/AgileMapper/blob/master/AgileMapper/MappingException.cs) and rethrown. To configure a mapper to swallow exceptions and return null instead, use:

```cs
Mapper.WhenMapping
    .SwallowAllExceptions();
```

Alternatively, to have a mapper call a callback in the event of an exception use:

```cs
Mapper.WhenMapping
    .PassExceptionsTo(ctx =>
    {
        Debug.Print(string.Format(
            "Error mapping from {0} to {1}: {2}",
            ctx.Source,
            ctx.Target,
            ctx.Exception));

        throw ctx.Exception;
    });
```

To only swallow exceptions thrown when mapping particular types, use:

```cs
Mapper.WhenMapping
    .From<PersonViewModel>() // Apply to PersonViewModel mappings (optional)
    .To<Person>()            // Apply to Person creation, updates and merges
    .SwallowAllExceptions();
```

...and to have a callback called for a particular type, use:

```cs
Mapper.WhenMapping
    .To<Person>()
    .PassExceptionsTo(ctx =>
        Debug.Log(new PersonException(ctx.Source.Id, ctx.Exception)));
```