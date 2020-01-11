# Crash Reporter

The `CrashReporter` is a small library intended to simplify the collection of
data at the time your program experiences issues. As a developer, you can
reference this library, and trigger collection routines that the user can then
send you in case of problems to assist with further debugging of your
application.

## Functionality At a Glance

This section lists the features supported by this library.

### General Functionality

Functionality which is available for all your software:

* Supports .NET 4.0 and .NET 4.5
  * Uses multitargetting, there are two solutions, one for .NET 4.0 and one for
    .NET 4.5.
* Dump various types of information to an XML file, that the user can send in
  addition to their bug reports.
  * Collects OS information (mostly Windows)
  * Collects all process environment variables
  * Gets the current .NET version installed and running
  * Lists all assemblies loaded into the program at the time of the crash
  * Lists all DLLs (modules) mapped into the program
  * Dump native thread information (there's no easy way to get .NET thread
    information)
* Log information to a memory backed logger, that is only written to disk when
  triggered by this library.

### Windows Specific

Functionality found on Windows only:

* Be able to create a crash dump, which can be used to analyze the state of your
  program using WinDbg.

## Data Collection

The library is intended to capture information that is useful for the developer
to further analyze the reason why software might not be working as expected. It
doesn't upload any data to any remote webserver on its own, and allows the user
to decide if they wish to provide this data as part of their bug report.

It puts the data in the folder `%LOCALAPPDATA%\CrashDumps`.

## Logging

A Trace Listener is implemented that uses memory for backing. This allows
tracing to occur, but without writing to disk. The only reason that traces are
written to disk is when there's a failure whereby the developer requires the
logs.

It implements a priority queue which is used to discard low priority messages
and to keep high priority messages. Typically at the time of a crash, the most
recent messages are of importance. But if there are warnings or errors, these
may be of more importance than the most recent.

## Extensibility

The software is designed to allow user provided data collection classes to
collect additional information that is not part of this library. You would
instantiate the class on start up and add it to the collection of crash data
collectors, so that when the time comes, it is called to dump the data to disk.

Create your own `ICrashProvider` and then add it to the list of providers.