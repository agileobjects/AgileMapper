Many-to-many relationships are sometimes (_e.g._ [EF Core 2](https://www.learnentityframeworkcore.com/configuration/many-to-many-relationship-configuration)) modelled using a joining type which joins one set of entities to another. For example:

```cs
class Course
{
    [Key]
    public int Id { get; set; }

    // One Course has many Students:
    public ICollection<CourseStudent> Students { get; set; }
}

class Student
{
    [Key]
    public int Id { get; set; }

    // One Student has many Courses:
    public ICollection<CourseStudent> Courses { get; set; }
}

// The join entity:
class CourseStudent
{
    public int CourseId { get; set; }
    public Course Course { get; set; }
    public int StudentId { get; set; }
    public Student Student { get; set; }
}
```

Often when projecting from entities to other types, we don't want the join entity, we want the _joined_ entity. For example:

```cs
class CourseDto
{
    public ICollection<StudentDto> Students { get; set; }
}
```

In this case, the `CourseDto` contains a collection of `StudentDto`s, not a collection of `CourseStudentDto`s.

AgileMapper recognises this sort of relationship and projects via the join entity to the joined Type - in this case `StudentDto`.

Projected joined entities are ordered using a join entity `Order` member if one exists, otherwise by using the joined Type's identifier. In this case because `CourseStudent` has no `Order` member, `Student.Id` will be used.