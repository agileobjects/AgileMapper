There are two circumstances in which you might want to keep track of mapped objects during a mapping:

### Circular References

To avoid stack overflows, objects are automatically tracked when mapping circular references. If you know your object tree doesn't contain a circular reference, you can switch this behaviour off. For example:

```C#
var child = new Child();
var parent = new Parent();
child.Parent = parent;
parent.Child = child;

// Objects reference each other, so allow automatic
// object tracking to handle the circular reference:
Mapper.Map(child).ToANew<Child>();

var child = new Child { Parent = new Parent() };

// Objects do not reference each other, so disable
// object tracking to boost performance:
Mapper.Map(child).ToANew<Child>(cfg => cfg.DisableObjectTracking());
```

### Identity Integrity

If you want to maintain a 1-to-1 relationship between source and destination objects, you can switch on identity integrity, which uses object tracking. For example:

```C#
var person = new Person();
// use the same instance twice in a source array:
var people = new[] { person, person };

var dtos1 = Mapper.Map(people).ToANew<PersonDto[]>();
// dtos1[0] and dtos1[1] will be different PersonDto instances

var dtos2 = Mapper
    .Map(people)
    .ToANew<PersonDto[]>(cfg => cfg.MaintainIdentityIntegrity());
// dtos2[0] and dtos2[1] will be the same PersonDto instance
```

### Global Settings

To switch object tracking off - or identity integrity on - in all mappings, use:

```C#
// Never track objects - when your object *types* contain circular references, 
// but you're sure your *instances* of those types never do:
Mapper.WhenMapping.DisableObjectTracking();

// Always maintain identity integrity:
Mapper.WhenMapping.MaintainIdentityIntegrity();
```