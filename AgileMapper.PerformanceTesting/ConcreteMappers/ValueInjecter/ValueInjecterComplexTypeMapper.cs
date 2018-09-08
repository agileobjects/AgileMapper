namespace AgileObjects.AgileMapper.PerformanceTesting.ConcreteMappers.ValueInjecter
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AbstractMappers;
    using Omu.ValueInjecter;
    using Omu.ValueInjecter.Injections;
    using static TestClasses.Complex;

    public class ValueInjecterComplexTypeMapper : ComplexTypeMapperBase
    {
        public override void Initialise()
        {
            Mapper.AddMap<List<Foo>, List<Foo>>(foos => foos != null ? Mapper.Map<List<Foo>>(foos) : new List<Foo>());
        }

        protected override Foo Clone(Foo foo)
            => (Foo)new Foo().InjectFrom<CloneInjection>(foo);

        public class CloneInjection : LoopInjection
        {
            protected override void Execute(PropertyInfo sp, object source, object target)
            {
                var tp = target.GetType().GetProperty(sp.Name);
                if (tp == null)
                {
                    return;
                }

                var val = sp.GetValue(source);

                tp.SetValue(target, GetClone(sp, val));
            }

            private static object GetClone(PropertyInfo sp, object val)
            {
                if (sp.PropertyType.IsValueType || sp.PropertyType == typeof(string))
                {
                    return val;
                }

                if (sp.PropertyType.IsArray)
                {
                    if (val == null)
                    {
                        return Array.CreateInstance(sp.PropertyType.GetElementType(), 0);
                    }

                    var arr = val as Array;
                    var arrClone = arr.Clone() as Array;

                    for (int index = 0; index < arr.Length; index++)
                    {
                        var a = arr.GetValue(index);
                        if (a.GetType().IsValueType || a is string)
                        {
                            continue;
                        }

                        arrClone.SetValue(Activator.CreateInstance(a.GetType()).InjectFrom<CloneInjection>(a), index);
                    }

                    return arrClone;
                }

                if (sp.PropertyType.IsGenericType)
                {
                    //handle IEnumerable<> also ICollection<> IList<> List<>
                    if (sp.PropertyType.GetGenericTypeDefinition().GetInterfaces().Contains(typeof(IEnumerable)))
                    {
                        var genericType = sp.PropertyType.GetGenericArguments()[0];
                        var listType = typeof(List<>).MakeGenericType(genericType);

                        if (val == null)
                        {
                            return Activator.CreateInstance(listType);
                        }

                        var list = Activator.CreateInstance(listType);

                        var addMethod = listType.GetMethod("Add");
                        foreach (var o in val as IEnumerable)
                        {
                            var listItem = genericType.IsValueType || genericType == typeof(string) ? o : Activator.CreateInstance(genericType).InjectFrom<CloneInjection>(o);
                            addMethod.Invoke(list, new[] { listItem });
                        }

                        return list;
                    }

                    //unhandled generic type, you could also return null or throw
                    return val;
                }

                if (val == null)
                {
                    return null;
                }

                //for simple object types create a new instace and apply the clone injection on it
                return Activator.CreateInstance(sp.PropertyType)
                                .InjectFrom<CloneInjection>(val);
            }
        }
    }
}