// ------------------------------------------------------------------------------
// <auto-generated>
// This code was generated by AgileObjects.AgileMapper.Buildable.
// Runtime Version: 0.1.0.0
// 
// Changes to this file may cause incorrect behavior and will be lost if
// the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------

using System;
using System.CodeDom.Compiler;
using AgileObjects.AgileMapper;
using AgileObjects.AgileMapper.ObjectPopulation;
using AgileObjects.AgileMapper.UnitTests.Common.TestClasses;

namespace AgileObjects.AgileMapper.Buildable.UnitTests.Mappers
{
    [GeneratedCode("AgileObjects.AgileMapper.Buildable", "0.1.0.0")]
    public class ChildMapper : MappingExecutionContextBase<Child>
    {
        public ChildMapper
        (
            Child source
        )
        : base(source)
        {
        }

        public Child ToANew<TTarget>()
            where TTarget : Child
        {
            return ChildMapper.CreateNew(this.CreateRootMappingData(default(Child)));
        }

        private static Child MapRepeated
        (
            IObjectMappingData<Child, Child> cToCData2
        )
        {
            try
            {
                Child child;

                if (cToCData2.TryGet(cToCData2.Source, out child))
                {
                    return child;
                }

                child = cToCData2.Target ?? new Child();
                cToCData2.Register(cToCData2.Source, child);
                child.Name = cToCData2.Source.Name;

                if (cToCData2.Source.EldestParent != null)
                {
                    child.EldestParent = ChildMapper.MapRepeated(
                        MappingExecutionContextBase<Child>.CreateChildMappingData(cToCData2.Source.EldestParent, child.EldestParent, cToCData2));
                }

                return child;
            }
            catch (Exception ex)
            {
                throw MappingException.For(
                    "CreateNew",
                    "Child.EldestParent.EldestChild",
                    "Child.EldestParent.EldestChild",
                    ex);
            }
        }

        private static Parent MapRepeated
        (
            IObjectMappingData<Parent, Parent> pToPData2
        )
        {
            try
            {
                Parent parent;

                if (pToPData2.TryGet(pToPData2.Source, out parent))
                {
                    return parent;
                }

                parent = pToPData2.Target ?? new Parent();
                pToPData2.Register(pToPData2.Source, parent);
                parent.Name = pToPData2.Source.Name;

                if (pToPData2.Source.EldestChild != null)
                {
                    parent.EldestChild = ChildMapper.MapRepeated(
                        MappingExecutionContextBase<Child>.CreateChildMappingData(pToPData2.Source.EldestChild, parent.EldestChild, pToPData2));
                }

                return parent;
            }
            catch (Exception ex)
            {
                throw MappingException.For(
                    "CreateNew",
                    "Child.EldestChild.EldestParent",
                    "Child.EldestParent.EldestChild.EldestParent",
                    ex);
            }
        }

        private static Child CreateNew
        (
            IObjectMappingData<Child, Child> cToCData
        )
        {
            Child sourceChild;
            try
            {
                sourceChild = cToCData.Source;

                var child = new Child();
                cToCData.Register(sourceChild, child);
                child.Name = sourceChild.Name;

                if (sourceChild.EldestParent != null)
                {
                    child.EldestParent = ChildMapper.GetParent(child, cToCData, sourceChild);
                }

                return child;
            }
            catch (Exception ex)
            {
                throw MappingException.For(
                    "CreateNew",
                    "Child",
                    "Child",
                    ex);
            }
        }

        private static Parent GetParent
        (
            Child child,
            IObjectMappingData<Child, Child> cToCData,
            Child sourceChild
        )
        {
            try
            {
                var parent = child.EldestParent ?? new Parent();
                cToCData.Register(sourceChild.EldestParent, parent);
                parent.Name = sourceChild.EldestParent.Name;

                if (sourceChild.EldestParent.EldestChild != null)
                {
                    parent.EldestChild = ChildMapper.MapRepeated(
                        MappingExecutionContextBase<Child>.CreateChildMappingData(sourceChild.EldestParent.EldestChild, parent.EldestChild, cToCData));
                }

                return parent;
            }
            catch (Exception ex)
            {
                throw MappingException.For(
                    "CreateNew",
                    "Child.EldestParent",
                    "Child.EldestParent",
                    ex);
            }
        }
    }
}