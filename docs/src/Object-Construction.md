AgileMapper constructs objects using [configured factories](/configuration/Object-Construction), factory methods and constructors, in that order.

### Factory Methods

A factory method will be used if:

- It's a public, static method returning an instance of the Type
- Its name starts with 'Create' or 'Get'
- All its parameters have [matching source values](/Member-Matching)

### Constructors

A constructor will be used if:

- It's public
- All its parameters have [matching source values](/Member-Matching)

### Selection Rules

- [Configured constructions](/configuration/Object-Construction) are preferred
- The factory method or constructor with the most parameters is preferred
- If a factory method has the same number of parameters as a constructor, the factory method is preferred
- If a factory method or constructor complex type argument would be passed as null, the next available factory method or constructor is used
- If there are no available factory methods or constructors with all-matched parameters - and no public, parameterless constructor - the member for which the object would be created is ignored
- To avoid infinite loops, if an object has a complex type constructor parameter of its own Type (a 'copy constructor'), it will be ignored.

If required, both [constructor parameters](/configuration/Constructor-Arguments) and [object construction](/configuration/Object-Construction) can be configured.