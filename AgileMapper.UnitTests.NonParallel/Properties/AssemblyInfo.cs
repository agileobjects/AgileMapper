using System.Reflection;
using System.Runtime.InteropServices;
using Xunit;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("AgileObjects.AgileMapper.UnitTests.NonParallel")]
[assembly: AssemblyDescription("AgileObjects.AgileMapper.UnitTests.NonParallel")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("AgileObjects Ltd")]
[assembly: AssemblyProduct("AgileObjects.AgileMapper")]
[assembly: AssemblyCopyright("Copyright ©  2015")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

[assembly: CollectionBehavior(DisableTestParallelization = true)]