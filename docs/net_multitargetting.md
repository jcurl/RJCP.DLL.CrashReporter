# Handling .NET Variants in Code

This module is implemented as a single set of source files that provide binaries
targetted for .NET 4.0 and .NET 4.5. The .NET 4.0 Framework is required to be
supported for some projects, but at the same time, it is desired to provide
features useful for newer versions of the framework.

* 1.0 At a Glance
  * 1.1 Main Differences
  * 1.2 Handling Differences
  * 1.3 Variables
  * 1.4 Care to be Taken
* 2.0 Detailed Handling of Multitargetting
  * 2.1 Naming of Project Files
  * 2.2 Providing Framework Variables for Conditional Compilation
  * 2.3 Conditional Compilation in Source Files
  * 2.4 Conditional Compilation in Project Files
  * 2.5 Output Paths and Intermediate Files
  * 2.6 Intermediate Files
  * 2.7 Assembly Configuration
  * 2.8 Importing NuGet Packages
    * 2.8.1 Package Reference
  * 2.9 NuSpec Packaging
  * 2.10 Application Configuration File
  * 2.11 Including in Multiple Solutions
    * 2.11.1 References found in the Internet

This document serves to identify:

* the structure of this project for multitargetting;
* how to add new files, packages, etc.;
* what should be observed in a code review.

## 1.0 At a Glance

### 1.1 Main Differences

Relevant differences between .NET 4.0 and .NET 4.5 for this project are:

* Task framework is very limited in .NET 4.0, no support for async/await which
  is an important feature for the CrashReporter.
* .NET 4.5 and above supports ZIP compression via `ZipArchive`. This doesn't
  exist in .NET 4.0, so an alternative external library needs to be found. It is
  not desirable to use the external libraries unless absolutely necessary (due
  to support, legacy, maintenance, etc.).

### 1.2 Handling Differences

There are two solution files, one for .NET 4.0 and the other for .NET 4.5. The
solutions just reference the .NET 4.0 and .NET 4.5 specific project files. This
uses the `packages.<project>.config` method for referencing NuGet packages,
particularly, the packages differ by the version of .NET they include as well as
including the packages only if required.

### 1.3 Variables

.NET 4.0 defines DEBUG and TRACE where necessary.

.NET 4.5 defines in addition the variable NET45, which can be used in code for
functionality that is specific to .NET 4.5 or later.

### 1.4 Care to be Taken

When adding new packages, new files, or modifying the `.csproj` file, special
care must be made to ensure that:

* Changes are made in both solutions / sets of project files;
* Both solutions compile.

## 2.0 Detailed Handling of Multitargetting

There are various ways of achieving multitargetting for a project with different
.NET Framework versions. The solution discussed here and provided by this
project uses multiple solutions, multiple project files and a single set of
source files. The project was first written for .NET 4.0, compiled to work, and
then extended for .NET 4.5. This section covers those details, and what needs to
be considered during development to ensure proper multitargetting.

When performing multitargetting, the minimum expected framework is .NET 4.0
Framework. This allows execution on Windows XP and above. Since .NET 4.0 on
Windows Vista and later, Microsoft provide a single set of libraries for all
versions up to .NET 4.8 (Windows XP only supports .NET 4.0 and as such is the
final version). The differences are the exposed API.

This makes multitargetting simpler as all functionality offered in .NET 4.0 are
present also in .NET 4.8. It does not however reduce effort for testing, if the
platform Windows XP is still expected to work, it must be tested.

### 2.1 Naming of Project Files

There is the standard convention that each project has the targetted framework
appended at the end of the names and project files:

* `CrashReporterApp-net40`
* `CrashReporterApp-net45`
* `RJCP.CrashReporter-net40`
* `RJCP.CrashReporter-net45`
* `RJCP.CrashReporterTest-net40`
* `RJCP.CrashReporterTest-net45`

### 2.2 Providing Framework Variables for Conditional Compilation

Each project file defines the variables `TRACE` and `DEBUG` in the
`<DefineConstants>` tag. An addition constant is defined for every .NET
Framework target (except for the base version, which is .NET 4.0). For example,
the .NET 4.5 Framework target project files (e.g.
`RJCP.CrashReporter-net45.csproj`) would define the variable `NET45`. If other
frameworks are to be targetted, e.g. .NET 4.6.2 or .NET 4.8, then those projects
would define additional variables (but only those variables which are required
by the project, not variables for all possible .NET versions).

* .NET 4.0 - no extra variables
* .NET 4.5 - Variable `NET45`
* .NET 4.6.2 - Variable `NET45` and `NET462`
* .NET 4.8 - Variable `NET45`, `NET462`, and `NET48`

There is no constant defined for the minimum framework (e.g. there is no
`NET40`), as this is the base target version.

### 2.3 Conditional Compilation in Source Files

Public API provided in libraries for .NET 4.0 must be made available in the same
form for all other newer framework versions also. But functionality for newer
frameworks may not necessarily be made avialable for older versions due to
differences in language features or compiler features.

For example, to provide async functionality for .NET 4.5 and later:

```csharp
public void LoadFile(string fileName) {
    ...
}

#if NET45
public async Task LoadFileAsync(string fileName) {
    ...
}
#endif
```

If functionality differs slightly between two target frameworks, use conditional
compilation with the `#else` statement:

```csharp
public void LoadFile(string fileName) {
    ...
#if NET45
    var t1 = myStream1.WriteAsync(buffer, 0, buffer.Length);
    var t2 = myStream2.WriteAsync(buffer, 0, buffer.Length);
    Task.WaitAll(t1, t2);
#else
    myStream1.Write(buffer, 0, buffer.Length);
    myStream2.Write(buffer, 0, buffer.Length);
#endif
    ...
}
```

As described in the previous section, the `NET45` should be defined for the .NET 4.5 Framework and *later*, so adding .NET 4.6.2 would still compile code for .NET 4.5.

### 2.4 Conditional Compilation in Project Files

In some circumstances, an entire class may be specific for a given target
framework. In this case, it makes little sense to wrap the entire class around a
conditional compilation clause. Instead, it would be easier to review if there
were a class for each target framework. The conditional compilation of that
class is then defined by the `<ItemGroup>` in the `.csproj` for the target
framework.

For example, there might be a class called `Compress`, which has two
implementations. The .NET 4.0 Framework implementation might be called
`Compress-ZipFile.cs` and the .NET 4.5 Framework implementation might be called
`Compress-ZipArchive.cs`. As both files define the same class, they cannot be
included simultaneously into the same project (else it would result in a
compilation failure).

This should be done only for internal implementations, and should not expose
directly public API.

If the API to the class were to change, both files need to be updated to ensure
compilation works as expected. But it does have the advantage of modifying a
single file and getting it to work for one Framework, and when successful and
all tests are complete, to then start work on the other target frameworks.

### 2.5 Output Paths and Intermediate Files

A standard project file can support multiple configurations but doesn't take
into account multiple frameworks. One can achieve this by duplicating the
configurations for each framework, but this can quickly explode in the number of
configurations defined, whereby multitargetting should really be orthogonal to
existing `Debug` and `Release` configurations (for each new framework added, one
must duplicate the existing configurations).

Each project file must be modified to change the output directory dependent on
the framework. For example:

```xml
  <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' ">
    ...
    <OutputPath>bin\Debug\net40\</OutputPath>
    <IntermediateOutputPath>obj\$(Configuration)\net40\</IntermediateOutputPath>
    ...
  </PropertyGroup>
  <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' ">
    ...
    <OutputPath>bin\Release\net40\</OutputPath>
    <IntermediateOutputPath>obj\$(Configuration)\net40\</IntermediateOutputPath>
    ...
  </PropertyGroup>
```

Similarly for .NET 4.5 project files, but with `net45` in the output path instead.

### 2.6 Intermediate Files

Visual Studio does not normally provide the `<IntermediateOutputPath>` in the
project file, where the default would be `obj\$(Configuration)`.

Leaving the default may result in build failures. The default behaviour of
Visual Studio is to do incremental builds. If the .NET 4.5 solution is loaded
and then built first, then the .NET 4.0 solution is loaded and then built, with
the intermediate objects put in the same directory, MSBuild would not rebuild
the object files a second time resulting in linkage failures (as timestamps of
the sources and the objects built have not changed since the last build). The
solution is to keep the intermediate files separated based on the framework.

Hence the addition of the tag `<IntermediateOutputPath>` as being required.

### 2.7 Assembly Configuration

Identifying the configuration used to build a library may be difficult (using
hashes, etc. against a database, if ever recorded) without the use of additional
metadata. To easily identify a compiled binary, the `AssemblyInfo.cs` should be
modified to generate metadata dependent on the project defined constants. For
example:

```csharp
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
```

### 2.8 Importing NuGet Packages

This documentation assumes VS2015 behaviour of using a NuGet `package.config`
file for each NuGet package that is required. As the project structure has
multiple `.csproj` files in a single directory (one project for each target),
there should be one package config file per .NET Framework target. NuGet
supports this by allowing the configuration file to be named
`package.<projectfile>.config`.

It is generally impractical to use only a single `package.config`, as it forces
the content to be defined for the lowest common denominator (.NET Framework 4.0)
and disallows using newer features for newer framework versions.

Having one `package.*.config` file per target thus allows the most flexibility
in defining which packages are needed as well as each version. It also allows
packages to be included only if needed for that target.

For example, `RJCP.CrashReporter` requires the package `DotNetZip` for .NET 4.0
Framework. It doesn't need this for .NET 4.5 Framework as it uses the system
provided `System.IO.Compression.FileSystem` namespace and references from the
GAC. This can only be achieved by using multiple package configuration files. So
it might be that the .NET 4.5 framework portion doesn't need a `packages.config`
file at all, only the .NET 4.0 Framework target.

When adding the first NuGet package or analyzer to a project, it will
automatically create `package.config`. This should then be renamed and the
`.csproj` file needs to be updated.

For example, the project file `RJCP.CrashReporter-net40.csproj` would have the
content:

```xml
  <ItemGroup>
    <None Include="packages.RJCP.CrashReporter-net40.config" />
  </ItemGroup>
```

The `<ItemGroup>` for the libraries will be updated to point to where NuGet
downloads and extracts the packages, which by default uses a relative path in
the `<HintPath>`. See later on the problems this imposes when including the
project file in multiple solutions.

As an example for the differences for the test project, we see the same versions
are used, but different DLLs are linked and distributed with the built binaries
(NUnit for example has different libraries for .NET 4.0 and .NET 4.5 making it
important to separate them with the usage of different `targetFramework`
attributes).

`packages.RJCP.CrashReporterTest-net40.config`
```xml
<?xml version="1.0" encoding="utf-8"?>
<packages>
  <package id="NUnit" version="3.12.0" targetFramework="net40" />
  <package id="NUnit.Analyzers" version="0.1.0-dev-00216" targetFramework="net40" />
  <package id="NUnit.ConsoleRunner" version="3.10.0" targetFramework="net40" />
</packages>
```

`packages.RJCP.CrashReporterTest-net45.config`
```xml
<?xml version="1.0" encoding="utf-8"?>
<packages>
  <package id="NUnit" version="3.12.0" targetFramework="net45" />
  <package id="NUnit.Analyzers" version="0.1.0-dev-00216" targetFramework="net45" />
  <package id="NUnit.ConsoleRunner" version="3.10.0" targetFramework="net45" />
</packages>
```

And for the main project, there is no `packages.RJCP.CrashReporter-net45.config`
as there are no NuGet packages provided, whereas the
`packages.RJCP.CrashReporter-net40.config` looks as follows:

```xml
<?xml version="1.0" encoding="utf-8"?>
<packages>
  <package id="DotNetZip" version="1.13.5" targetFramework="net40" />
</packages>
```

The `packages.RJCP.CrashReporter-net45.csproj` also doesn't reference `DotNetZip`.

#### 2.8.1 Package Reference

Visual Studio 2017 and later support project files with a `<PackageReference>`
in the `.csproj` file. This hasn't been tested well enough to document here, and
is not supported by the current `RjBuild` tool (v0.2.2).

### 2.9 NuSpec Packaging

NuGet supports multiple targets in a single package. This project supports
creating a single `RJCP.CrashReporter.nuget` package that has binaries for .NET
4.0 and .NET 4.5. To do this, framework dependent dependencies need to be
defined and need to match the content of the `packages.*.config`.

For example

```xml
<?xml version="1.0" encoding="utf-8"?>
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
    <metadata>
        <id>RJCP.CrashReporter</id>
        ...
        <dependencies>
            <group targetFramework=".NETFramework4.0">
                <dependency id="DotNetZip" version="1.13.5" />
            </group>
        </dependencies>
    </metadata>
    ...
</package>
```

This says that only the .NET 4.0 generated binaries depend on `DotNetZip` and
.NET 4.5 has no dependencies (as it uses the built in functionality of the
system).

### 2.10 Application Configuration File

Rarer are applications which are built for multitargetting. A project file can
only have one `App.config` file, which is renamed to be the name of the
executable binary, e.g. `CrashReportApp.exe.config`. The `App.config` also
specifies the target framework, and it must be different depending on the
target.

For example, building `CrashReportApp` for .NET 4.0 requires the following content:

```xml
  <startup>
    <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.0" />
  </startup>
```

For .NET 4.5 it must be:

```xml
  <startup>
    <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.5" />
  </startup>
```

If the .NET 4.5 target links against the `sku` for .NET 4.0 with
`.NETFramework,Version=v4.0`, it will run, but one will observe that the Windows
Operating system will automatically load the library `apphelp.dll` into the
process. It will not load it if the `sku` is against
`.NETFramework,Version=v4.5`. The `apphelp.dll` library on Windows provides
shims for minor incompatibilities between programs to prevent them from
crashing.

But if the .NET 4.0 version has the `sku` for .NET 4.5, it will not run on
Windows XP, even if the binaries are compiled against .NET 4.0 and says so in
the PE header. Windows XP will ask if .NET 4.5 should be downloaded and
installed, for which it doesn't exist.

To provide the correct `App.config` depending on the target, the `.csproj` files
for each target framework need to be modified, to add a target for after
building to copy the correct application configuration:

`CrashReportApp-net40.csproj`
```xml
  <Target Name="AfterBuild">
    <Delete Files="$(TargetDir)$(TargetFileName).config" />
    <Copy SourceFiles="$(ProjectDir)\App.net40.config" DestinationFiles="$(TargetDir)$(TargetFileName).config" />
  </Target>
```

and

`CrashReportApp-net45.csproj`
```xml
  <Target Name="AfterBuild">
    <Delete Files="$(TargetDir)$(TargetFileName).config" />
    <Copy SourceFiles="$(ProjectDir)\App.net45.config" DestinationFiles="$(TargetDir)$(TargetFileName).config" />
  </Target>
```

The above examples require there be an `App.net40.config` and a
`App.net45.config` as required.

One must remember, changes in one configuration file should be placed in the
other configuration files also (unless they're framework dependent).

### 2.11 Including in Multiple Solutions

All documentation prior has assumed only a single set of solution files (one per
target framework).

* `CrashReporter-net40.sln`
* `CrashReporter-net45.sln`
* `CrashReportApp\`
  * `CrashReportApp-net40.csproj`
  * `CrashReportApp-net45.csproj`
* `CrasReporter\`
  * `RJCP.CrashReporter-net40.csproj`
  * `RJCP.CrashReporter-net45.csproj`
* `CrashReporterTest\`
  * `RJCP.CrashReporterTest-net40.csproj`
  * `RJCP.CrashReporterTest-net45.csproj`

For libraries, it is useful for other solution files to include the `.csproj`
files directly. This can reduce debugging effort significantly.

For this to work, the `.csproj` files must be modified so that references to
NuGet packages work as intended. By default, references to NuGet packages have a
`<HintPath>` which uses a relative path from the current project file, to where
the packages are extracted by NuGet. Usually, the packages extracted by NuGet
are in a folder called `packages` alongside of the solution. So if a project is
included by two solutions, and the two solutions are in different folders,
restoring packages and building will only work for one solution.

To fix this, the relative paths must be removed and replaced with variables
provided by MsBuild.

For NuGet libraries, change the directory for the hint to use `$(SolutionDir)`.

```xml
  <ItemGroup>
    <Reference Include="nunit.framework, Version=3.12.0.0, Culture=neutral, PublicKeyToken=2638cd05610744eb, processorArchitecture=MSIL">
      <HintPath>$(SolutionDir)\packages\NUnit.3.12.0\lib\net40\nunit.framework.dll</HintPath>
    </Reference>
```

For Analyzers, change the directory for the include to use `$(SolutionDir)`.

```xml
  <ItemGroup>
    <Analyzer Include="$(SolutionDir)\packages\NUnit.Analyzers.0.1.0-dev-00216\analyzers\dotnet\cs\nunit.analyzers.dll" />
  </ItemGroup>
```

NUnit3 Test projects import extra properties, which also needs to be changed also.

```xml
  <Import Project="$(SolutionDir)\packages\NUnit.3.12.0\build\NUnit.props" Condition="Exists('$(SolutionDir)\packages\NUnit.3.12.0\build\NUnit.props')" />
```

#### 2.11.1 References found in the Internet

There is a blog [1] on the Internet which describes a similar scenario as this
section. But it was written in 2013, and it doesn't solve the problem of the
analyzers having their path hardcoded to where NuGet extracts the files (which
was introduced with Roslyn with Visual Studio 2017).

Do not have a `nuget.config` file that changes the path of the packages to
somewhere else other where the solution file is.

```xml
<configuration>
  <config>
    <!-- Defines a common package repository for all solutions -->
    <add key="repositoryPath" value=".\Packages" />
  </config>
</configuration>
```

The `.csproj` files have been modified to look for the packages at
`$(SolutionDir)`. Doing so will work only for solution files that have the
`packages` folder in parallel.

* `project/`
  * `myproject.sln`
  * `packages/`

But not this, as the packages is not next to the current solution file.

* `globalproject/`
  * `project/`
    * `myproject.sln`: This solution will have packages extracted to a different
      directory than the solution directory.
  * `nuget.config`: Defines the packages folder to be in this current directory.
  * `packages/`

References:

* [1] https://www.damirscorner.com/blog/posts/20130527-NuGetPackageRestoreForProjectsInMultipleSolutions.html
* [2] https://stackoverflow.com/questions/18376313/setting-up-a-common-nuget-packages-folder-for-all-solutions-when-some-projects-a