using System;
using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("CrashReporter")]
[assembly: AssemblyDescription("Crash Report Support Library")]
#if NET45
#if DEBUG
[assembly: AssemblyConfiguration("debug-net45")]
[assembly: AssemblyProduct("CrashReporter NET 4.5 (Debug)")]
#else
[assembly: AssemblyConfiguration("net45")]
[assembly: AssemblyProduct("CrashReporter NET 4.5")]
#endif
#else
#if DEBUG
[assembly: AssemblyConfiguration("debug-net40")]
[assembly: AssemblyProduct("CrashReporter NET 4.0 (Debug)")]
#else
[assembly: AssemblyConfiguration("net40")]
[assembly: AssemblyProduct("CrashReporter NET 4.0")]
#endif
#endif
[assembly: AssemblyCompany("Jason Curl")]
[assembly: AssemblyCopyright("Copyright © Jason Curl 2019-2020")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

[assembly: CLSCompliant(true)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("807b678b-ccbd-40f5-bf8c-88862c80d44e")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("0.4.2.0")]
[assembly: AssemblyFileVersion("0.4.2.0")]
[assembly: AssemblyInformationalVersion("0.4.2.0")]
