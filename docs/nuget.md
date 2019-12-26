# NuGet

## Restoring Packages

This project is referenced by two different solution files - the one in this
repository, and another in a bigger, more global repository for building more
packages (which reference this package).

As such, to allow the analyzers as well as nuget packages to be found, the
`.csproj` file has been modified to remove the hard coded paths and to replace
it with the `$(SolutionDir)` variable.

There is a blog [1] on the Internet which describes this. But it was written in
2013, and it doesn't solve the problem of the analyzers having their path
hardcoded to where NuGet extracts the files.

## The `NuGet.config` file

Do not have a `nuget.config` file that changes the path of the packages to
somewhere else other where the solution file is. The `.csproj` files have been
modified to look for the packages at `$(SolutionDir)`. Doing so will work
only for solution files that have the `packages` folder in parallel.

* `project/`
  * `myproject.sln`
  * `packages/`

But not this, as the packages is not next to the current solution file.

* `globalproject/`
  * `project/`
    * `myproject.sln`
  * `nuget.config`
  * `packages/`

## Further References

[1] https://www.damirscorner.com/blog/posts/20130527-NuGetPackageRestoreForProjectsInMultipleSolutions.html
[2] https://stackoverflow.com/questions/18376313/setting-up-a-common-nuget-packages-folder-for-all-solutions-when-some-projects-a