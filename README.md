# Crash Reporter <!-- omit in toc -->

The `CrashReporter` is a small library intended to simplify the collection of
data at the time your program experiences issues. As a developer, you can
reference this library, and trigger collection routines that the user can then
send you in case of problems to assist with further debugging of your
application.

- [1. Functionality At a Glance](#1-functionality-at-a-glance)
  - [1.1. General Functionality](#11-general-functionality)
  - [1.2. Windows Specific](#12-windows-specific)
- [2. Data Collection](#2-data-collection)
- [3. Logging](#3-logging)
- [4. Extensibility](#4-extensibility)
- [5. Example Code](#5-example-code)
- [6. Dependencies](#6-dependencies)
- [7. Release History](#7-release-history)
  - [7.1. Version 0.5.1](#71-version-051)
  - [7.2. Version 0.5.0](#72-version-050)
  - [7.3. Version 0.4.3](#73-version-043)
  - [7.4. Version 0.4.2](#74-version-042)
  - [7.5. Version 0.4.1](#75-version-041)
  - [7.6. Version 0.4.0.1](#76-version-0401)
  - [7.7. Version 0.4.0](#77-version-040)


## 1. Functionality At a Glance

This section lists the features supported by this library.

### 1.1. General Functionality

Functionality which is available for all your software:

- Supports .NET 4.0 and .NET 4.5 to 4.8, and .NET Core (.NET Standard 2.1)
- Dump various types of information to an XML file, that the user can send in
  addition to their bug reports.
  - Collects OS information (mostly Windows)
  - Collects all process environment variables
  - Gets the current .NET version installed and running
  - Lists all assemblies loaded into the program at the time of the crash
  - Lists all DLLs (modules) mapped into the program
  - Dump native thread information (there's no easy way to get .NET thread
    information)
- Log information to a memory backed logger, that is only written to disk when
  triggered by this library.

### 1.2. Windows Specific

Functionality found on Windows only:

- Be able to create a crash dump, which can be used to analyze the state of your
  program using WinDbg.

## 2. Data Collection

The library is intended to capture information that is useful for the developer
to further analyze the reason why software might not be working as expected. It
doesn't upload any data to any remote webserver on its own, and allows the user
to decide if they wish to provide this data as part of their bug report.

It puts the data in the folder `%LOCALAPPDATA%\CrashDumps`.

You can override the location for where logs are captured in your application
configuration file.

## 3. Logging

A Trace Listener is implemented that uses memory for backing. This allows
tracing to occur, but without writing to disk. The only reason that traces are
written to disk is when there's a failure whereby the developer requires the
logs.

It implements a priority queue which is used to discard low priority messages
and to keep high priority messages. Typically at the time of a crash, the most
recent messages are of importance. But if there are warnings or errors, these
may be of more importance than the most recent.

On .NET Core, add the loggers using the `.AddSimplePrioMemoryLogger()` method.

## 4. Extensibility

The software is designed to allow user provided data collection classes to
collect additional information that is not part of this library. You would
instantiate the class on start up and add it to the collection of crash data
collectors, so that when the time comes, it is called to dump the data to disk.

Create your own `ICrashProvider` and then add it to the list of providers.

## 5. Example Code

For a minimalistic example of how to get it working in your own project, see the
project `CrashReportApp`, which shows how to configure the application
configuration file for .NET 4.x for both a watchdog and tracing, and how to
configure the `LogSource` explicitly for .NET Core 3.1 without having to use
complicated dependency injection.

See also the `docs` directory in the original repository for more detailed
information.

## 6. Dependencies

This library also depends on my `RJCP.Diagnostics.Trace` library to provide a
unified logging experience between .NET Framework and .NET Core, via the
`LogSource` class.

## 7. Release History

### 7.1. Version 0.5.1

Quality:

- Update README.md reference in NuGet package (DOTNET-812)
- Update .NET 4.5 to 4.6.2 (DOTNET-827)
- Update .NET Standard 2.1 to .NET 6.0 (DOTNET-833, DOTNET-936, DOTNET-938,
  DOTNET-942, DOTNET-944, DOTNET-945, DOTNET-952, DOTNET-959)
- Update to .NET 8.0 (DOTNET-982, DOTNET-983, DOTNET-989, DOTNET-990)
- Clean up code (DOTNET-946)
- Only delete old files if they're at least five minutes old (DOTNET-969)
- Enable NetFX Runtime version information on .NET 4.8, as well as .NET Core
  (DOTNET-976)

### 7.2. Version 0.5.0

Feature:

- Add support for .NET Standard 2.1 (DOTNET-355)
- Add logging using .NET Core Logging (DOTNET-364)
- Get runtime framework information from the .NET Core runtime (DOTNET-366)
- .NET Frameworks for Windows 20H2, 21H1 and Windows 11 added (DOTNET-453)
- Extract the Windows UBR from the registry (DOTNET-459)
- Provide a .NET Core ILogger for Memory Logs (DOTNET-558)
- Capture core dumps with exception information on .NET Core (DOTNET-572)
- Use undocumented API from WinBrand to get Windows Version (DOTNET-633)
- Use ExceptionDispatchInfo when raising inner exceptions (DOTNET-782)

Bugfixes:

- Fix usage of `GetNativeSystemInfo`, `GetSystemInfo` (DOTNET-383)
- Allow core dumps on Windows XP (DOTNET-636)
- Create file permissions in ZIP with .NET 4.6.1 and later (DOTNET-646)

Quality:

- Upgrade to .NET SDK project style (DOTNET-343, DOTNET-351, DOTNET-360, DOTNET-637)
- Authenticode sign the binaries (DOTNET-353, DOTNET-367)
- Build with GIT version information (DOTNET-375)
- Code Cleanup (DOTNET-383, DOTNET-558, DOTNET-637, DOTNET-717, DOTNET-804)
- Use `IsWow64Process2` on newer Windows (DOTNET-383)
- Runtime Information support for .NET 4.7.1 and later
- SimplePrioMemoryLog: Don't allow a log group with 0 entries (DOTNET-568)
- Increase level of some trace messages (DOTNET-570)
- Ignore FirstChanceExceptions when dumping data (DOTNET-575)
- Use a RJCP.Core.Environment for the platform (DOTNET-729)

### 7.3. Version 0.4.3

Quality:

- Correct documentation (HELIOS-1661)

Bugfixes:

- Sanitise output to XML on output for async methods (HELIOS-1663)
- Handle file system changes while cleaning (HELIOS-1664)

### 7.4. Version 0.4.2

Bugfixes:

- Properly detect .NET 4.5 (HELIOS-1371)
- Fix usage of registry on Windows with 32-bit .NET on 64-bit OS (HELIOS-1372)
- Sanitise output to XML on output (HELIOS-1415)

Quality:

- Update detection of .NET 4.8 on Windows 1909 (HELIOS-1370)
- Print exception type in the log (HELIOS-1418)

### 7.5. Version 0.4.1

Features:

- Allow configuration of dump directory (HELIOS-1347)

Bugfixes:

- Suppress NetSupportedException for dynamic assemblies (HELIOS-1352)
- Capture Access Denied exceptions in ThreadDump (HELIOS-1350)

Quality:

- Remove useless stylesheet set property (HELIOS-1349)

### 7.6. Version 0.4.0.1

- Update NuSpec

### 7.7. Version 0.4.0

- Initial Release