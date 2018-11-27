Derived type pairing matches particular derived sources types to particular derived target types. Most of the time [this is done automatically](/Derived-Types), but if your derived types aren't consistently-named, or you want to make derived type mapping conditional, you can configure it.

For example:

```cs
Mapper.WhenMapping
   .From<AnimalViewModel>() // Apply to AnimalViewModel mappings
   .ToANew<Animal>()        // Apply to Animal creation only
   .Map<CanineViewModel>()  // If the source is a CanineViewModel instance
   .To<Dog>();              // Create an instance of Dog
```

Type pairs are used when mapping complex types:

```cs
var animalViewModel = new CanineViewModel() as AnimalViewModel;
var animal = Mapper.Map(animalViewModel).ToANew<Animal>();
// animal is of type Dog
```

...and collections:

```cs
var animalViewModels = new[]
{
    new AnimalViewModel(),
    new CanineViewModel()
};
var animals = Mapper.Map(animalViewModels).ToANew<List<Animal>>();
// animals[0] is of type Animal
// animals[1] is of type Dog
```

...and can be applied conditionally ([inline](/configuration/Inline) example):

```cs
Mapper
    .Map(animalViewModels).ToANew<Animal[]>(cfg => cfg
        .WhenMapping
        .From<AnimalViewModel>()
        .ToANew<Animal>()
        .If((avm, a) => avm.Sound == "Woof") // If the animal woofs
        .MapTo<Dog>());                      // Create an instance of Dog
```

Conditional derived type mappings can also be configured [for Dictionaries](/configuration/Dictionary-Mapping#conditional-derived-types).