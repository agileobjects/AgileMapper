For most cases, derived types are supported without configuration.

For example, with the following classes:

```C#
public class Person {}
public class Customer : Person
{
    public float Discount { get; set; }
}

public class PersonViewModel {}
public class CustomerViewModel : PersonViewModel
{
    public double Discount { get; set; }
}
```

...the derived `Customer` type is recognised and mapped to a `CustomerViewModel`, even though the source variable is of type `Person` and the requested target type is `PersonViewModel`:

```C#
var person = new Customer { Discount = 0.1f } as Person;
var viewModel = Mapper.Map(person).ToANew<PersonViewModel>();
// viewModel is of type CustomerViewModel
// viewModel.Discount is 0.1
```

Derived types are also paired up automatically - for example, with the following classes:

```C#
public class Animal {}
public class Dog : Animal {}
public class Cat : Animal {}

public class AnimalDto {}
public class DogDto : AnimalDto {}
public class CatDto : AnimalDto {}
```

The following mappings are performed:

```C#
var sourceAnimal = new Dog() as Animal;
var resultAnimal = Mapper.Map(sourceDog).ToANew<Animal>();
// resultAnimal is of type Dog

var sourceAnimals = new Animal[] { new Cat(), new Dog() };
var resultAnimals = Mapper.Map(sourceAnimals).ToANew<AnimalDto[]>();
// resultAnimals[0] is of type CatDto
// resultAnimals[1] is of type DogDto
```

In the second example above, the `Dog` -> `DogDto` and `Cat` -> `CatDto` types are paired by convention based on the names of the original `Animal` -> `AnimalDto` pairing. You can configure [type pairs](Pairing-Derived-Types) which don't have a consistent naming convention, and [in which assemblies](Assembly-Scanning) to look for derived types.