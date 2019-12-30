# Handling .NET Variants in Code

This module is implemented to provide sources for .NET 4.0 and .NET 4.5. There
are significant differences between the frameworks and .NET 4.0 must be
supported, but at the same time, features for more modern versions of .NET
should be provided.

## Main Differences

The differences between .NET 4.0 and .NET 4.5 are:

* Task framework is very limited in .NET 4.0, no support for async/await which
  is an important feature for the CrashReporter.
* .NET 4.5 and above supports ZIP compression, which .NET 4.0 doesn't, so there
  is some code implemented to make it work for .NET 4.0, but where possible, use
  framework functionality for .NET 4.5.

## Handling Differences

There are two solution files, one for .NET 4.0 and the other for .NET 4.5. The
solutions just reference the .NET 4.0 and .NET 4.5 specific project files. This
uses the `packages.config` method for referencing NuGet packages, particularly,
the packages differ by the version of .NET they include.

### Variables

.NET 4.0 defines DEBUG and TRACE where necessary.

.NET 4.5 defines in addition the variable NET45, which can be used in code for
functionality that is specific to .NET 4.5 or later.

### Care to be Taken

When adding new packages, new files, or modifying the `.csproj` file, especial
care must be made to ensure that:
* Changes are made in both solutions / sets of project files;
* Both solutions compile.