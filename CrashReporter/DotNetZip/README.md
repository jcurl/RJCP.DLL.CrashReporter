# DotNetZip

The NuGet package of DotNetZip 1.16.0 has the CVE vulnerability CVE-2024-48510.
The original DotNetZip package is now out of support and will not receive any
further updates to resolve this vulnerability.

You'll see the downloaded version from GIT (commit 8d8ad18). It has been updated
to remove the references for Android, iOS and .NET Standard which are not needed
for CrashReporter.

The rebuild is done on Windows 7 SP1, with VS2015 U3 Patch 1 against .NET 4.0.

Minimal changes have been made:

- Rename the assembly from `DotNetZip` to `RJCP.DotNetZip` to make it clear it
  is now forked.
- Update the version from `1.16.0` to `1.16.0.1`

## Building

Preparations before building

1. Windows 7 SP1 x64 was used as the build environment.

2. VS2015 with Update 3 Patch 1 was installed.

3. An older version of `ILMerge.msi` is downloaded, instead of using the NuGet
   package. Version v2.10.526.0. I've copied the installer to this repository as
   well, as it's difficult to find (the link in the DotNetZip repo is dead).

Instructions to build:

1. Run the Visual Studio 2015 Developer Prompt.

2. Go to the directory where the sources are. They were copied from this
   repository to the target machine where the build is being done.

   ```cmd
   cd \repos\DotNetZip.Semverd
   ```
   
3. Build the Release Version

   ```cmd
   msbuild "src\Zip\Zip DLL.csproj" /p:Configuration=Release
   ```
   
4. Confirm the built version

   You should be able to see that it was the release version that was built when
   using tools like dotPeek to read the metadata.
   
   Viewing the properties of the file and then details should also show the
   correct version number of the library.

## Patching Crash Reporter

Copy the resulting `RJCP.DotNetZip.dll` and `RJCP.DotNetZip.pdb` files into this
folder, that the `.csproj` files pick it up and copy to the destination folder.
