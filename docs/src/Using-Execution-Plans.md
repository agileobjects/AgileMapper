Mappings are performed by creating an [expression tree](https://msdn.microsoft.com/en-us/library/mt654263.aspx), then compiling it to a function which is cached and executed. The cached function is the mapping execution plan.

### Caching execution plans

To cache an execution plan, use:

```C#
// Cache the plan to map a PersonViewModel to a new instance of a Person:
Mapper.GetPlanFor<PersonViewModel>().ToANew<Person>();

// Cache the plan to map a Person to a new instance of a PersonViewModel;
// map Person.Title + " " + Person.Name to PersonViewModel.Name:
Mapper
    .GetPlanFor<Person>()
    .ToANew<PersonViewModel>(cfg => cfg
        .Map((p, pvm) => p.Title + " " + p.Name)
        .To(pvm => p.Name));

// Cache the plans to map a PersonViewModel:
//  - to a new instance of a Person
//  - over an existing instance of a Person (an update)
//  - onto an existing instance of a Person (a merge)
Mapper.GetPlansFor<PersonViewModel>().To<Person>();

// Cache the plan to project a Person to a PersonViewModel;
// Projection plans are cached against the QueryProvider, so
// an instance of the queryable being projected is required:
Mapper.GetPlanForProjecting(context.People).To<PersonViewModel>();
```

By default, execution plans are created and cached the first time two types of objects are mapped - this is a _relatively_ expensive operation, so if you'd like to choose when to incur the cost (on app start-up, for example), you can use a set of calls like the above.

### Viewing execution plans

Viewing an execution plan can help you:

- See what will and won't be mapped
- See how your configuration is applied
- Chase down any unexpected behaviour

To view a plan, assign the result of the call to an explicitly-typed string variable and view it in the debugger:

```C#
string plan = Mapper.GetPlanFor<PersonViewModel>().ToANew<Person>();
```

To view all the plans cached in a mapper, use:

```C#
Mapper.GetPlansFor<Person>.ToANew<PersonViewModel>();
Mapper.GetPlansFor<PersonViewModel>.To<Person>();

// View the four cached plans:
//  - Person to a new PersonViewModel
//  - PersonViewModel to a new Person
//  - PersonViewModel updating an existing Person
//  - PersonViewModel merging onto an existing Person
var allPlans = Mapper.GetPlansInCache();
```

An example plan looks like this:

![An example mapping plan](https://gwb.blob.core.windows.net/mrsteve/Style-images-placeholder_173352/MappingPlan_45676864.gif)