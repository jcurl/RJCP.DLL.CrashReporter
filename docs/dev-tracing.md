# Tracing using Crash Reporter

## Why use Crash Reporter

.NET Framework 4.x provides trace functionality, with the framework providing
`TraceListeners` that can log to the console or to a file. The .NET Core
provides an `ILogger` via a `ILoggerFactory` that has similar functionality.
They append logs as required. Most of the time these logs are not required, can
slow down your application slightly while writing to disk and more importantly,
fill up the space on the users harddisk, pushing the onus onto the user to
remove unneeded trace files, or to disable traces (and so have no possibility to
report issues without reenabling and spending significant effort in
reproduction).

This library provides a `TraceListener` implementation that instead allows
traces to be logged to RAM initially, using a prioritized queue to discard old
traces as RAM usage increases, and only writes them to disk when instructed to
do so. This means traces are only saved as they're required (when there's a
problem).

The `LogSource` class from the `RJCP.Diagnostics.Trace` library provides the
singleton required that `RJCP.Diagnostics.CrashReporter` can use to trace to a
trace listener (or your own `TraceListener` or `ILogger`).

## Quick Guide

### .NET Framework 4.x

On .NET Framework 4.x ,reference the `RJCP.Diagnostics.CrashReporter` library
into your project, and modify the `App.config` file to include the following:

```xml
  <system.diagnostics>
    <sharedListeners>
      <add name="myListener" type="RJCP.Diagnostics.Trace.SimplePrioMemoryTraceListener, RJCP.Diagnostics.CrashReporter"/>
      <add name="console" type="System.Diagnostics.ConsoleTraceListener"/>
    </sharedListeners>

    <sources>
      <source name="RJCP.CrashReporter" switchValue = "Warning">
        <listeners>
          <remove name="Default"/>
          <add name="myListener"/>
        </listeners>
      </source>
      <source name="RJCP.CrashReporter.Watchdog" switchValue = "Verbose">
        <listeners>
          <remove name="Default"/>
          <add name="myListener"/>
        </listeners>
      </source>
      <source name="CrashReporterApp" switchValue="Verbose">
        <listeners>
          <remove name="Default"/>
          <add name="myListener"/>
          <add name="console"/>
        </listeners>
      </source>
    </sources>
    <trace autoflush="true" useGlobalLock="false"/>
  </system.diagnostics>
```

.NET Core doesn't read the `app.config` file, which is why it doesn't work for
those projects.

Replace `CrashReporterApp` with the name of your applications `TraceSource`
name. The `RJCP.CrashReporter` and `RJCP.CrashReporter.Watchdog` should be
present, so that when there is a problem performing a crash dump, details will
be printed to the console.

This will capture all traces internally in RAM, and only output them to disk
when instructed to by your program.

```csharp
using System;
using System.IO;
using RJCP.Diagnostics;
using RJCP.Diagnostics.Dump;
using RJCP.Diagnostics.Trace;

namespace CrashReportApp
{
    static class Program
    {
        static void Main()
        {
            CrashReporter.SetExceptionHandlers();

            Log.App.TraceEvent(System.Diagnostics.TraceEventType.Information, "Program Started");

            // Dump all traces (no core dump) and information to the current directory
            string path = Path.Combine(Environment.CurrentDirectory, Crash.Data.CrashDumpFactory.FileName);
            Crash.Data.Dump(path);

            // Now simulate an unhandled exception, which creates a core dump
            throw new InvalidOperationException("An exception which should cause a dump");
        }
    }
}
```

### .NET Core (.NET Standard 2.1)

The .NET Core framework doesn't read the `app.config` file. While `TraceSource`
is part of the sources and is available, there is no singleton initializing the
tracing framework, and the client application writer must do this themselves.
For this library it is sufficient to instantiate the `MemoryTraceListener` in
code and assign to to the `LogSource` as such:

```csharp
using System.Diagnostics;
using RJCP.Diagnostics.Trace;

public static class Log
{
    public static LogSource App { get; private set; }

    static Log()
    {
#if NETCOREAPP
        SimplePrioMemoryLog log = new SimplePrioMemoryLog() {
            Critical = 100,
            Error = 150,
            Warning = 200,
            Info = 250,
            Verbose = 300,
            Other = 100,
            Total = 1500
        };
        MemoryTraceListener listener = new MemoryTraceListener(log);
        LogSource.SetLogSource("CrashReporterApp", SourceLevels.Verbose, listener);
        LogSource.SetLogSource("RJCP.CrashReporter", SourceLevels.Verbose, listener);
        LogSource.SetLogSource("RJCP.CrashReporter.Watchdog", SourceLevels.Verbose, listener);
#endif
        App = new LogSource("CrashReporterApp");
    }
```

You can of course get the `ILogger` object on .NET Standard through the
`App.Logger` property. See the `RJCP.Diagnostics.Trace` project for more
details.

## Detailed Information

### The Trace Prioritized Queue

The implementation uses a prioritized queue with a priority based on the log
severity (in order given, the first being the highest priority):

| Trace Level                  | Default Queue Size (msgs) |
|------------------------------|---------------------------|
| `TraceEventType.Critical`    | 200                       |
| `TraceEventType.Error`       | 200                       |
| `TraceEventType.Warning`     | 300                       |
| `TraceEventType.Information` | 400                       |
| `TraceEventType.Verbose`     | 500                       |
| Others                       | 200                       |
| Total                        | 2000                      |

Only when the maximum (Total) number of messages have been logged will messages
be discarded. Messages of the lowest priority will be discarded first, up to the
priority of the new message being logged.

The assumption is that at the time of the crash information which is nearest to
the time of the crash are most important. But sometimes old information such as
warnings or errors may be relevant (such as startup) which have an effect later
on in the program. Thus, it is guaranteed there will be a minimum number of
those types of messages being logged requiring the prioritized queue.

### Redefining the Queue Size

An application configuration can override the amount of messages that can be
logged in the `App.config` file.

```xml
    <sharedListeners>
      <add name="myListener" type="RJCP.Diagnostics.Trace.SimplePrioMemoryTraceListener, RJCP.Diagnostics.CrashReporter"
        Critical="100" Error="150" Warning="200" Information="250" Verbose="300" Other="100" Total="1500"/>
    </sharedListeners>
```

The attributes defined in the configuration file define the size of the queues.
If the Total is less than the sum of all the other queues, it will be implicitly
be set to the minimum required to accomodate all the queues. If it is larger,
then messages will be discarded later.

### Logging Multiple Trace Sources into a Single Listener (.NET Framework 4.x)

Use the `<sharedListeners>` for each `<source>` in the `App.config`. Logging
will be placed into a single block.

### Performing a Dump Programmatically

To execute a dump to access the trace information (among other information), add
the following in your code:

```csharp
string fileName = CrashReporter.CreateDump(RJCP.Diagnostics.Dump.CoreType.None);
Console.WriteLine("DumpFile: {0}", fileName);
```

The output is generally placed into `%LOCALAPPDATA%\CrashDumps` and is a ZIP
file with the relevant information, named after the date/time and a GUID (to
avoid collisions).

Default behaviour is to create a XML file with the content.

### Performing a Dump on Unhandled Exceptions

Recommended guidelines is to only capture exceptions which are known. Unhandled
exceptions are then caught by the application domain. To create a dump when an
exception is unhandled:

```csharp
static void Main() {
    CrashReporter.Source = new TraceSource("CrashReporterApp");
    CrashReporter.SetExceptionHandlers();

    DoWork();
}
```

The `SetExceptionHandlers` will register the `AppDomain.UnhandledException` and
the `AppDomain.FirstChanceException`. On Mono, the `FirstChanceException` is not
defined, so this library uses reflection to register the handlers, so your code
will continue to compile also on Mono.

#### XML Crash Dump Logging

All errors when writing the XML crash dump file are logged to `RJCP.CrashReporter`.

#### Crash Reporter

* First Chance Exceptions are logged to the `CrashReporter.Source` property.
* Unhandled Exceptions are logged to the `CrashReporter.Source` property.
* The actual creation of crash dumps (either success or failure) are logged to
  `RJCP.CrashReporter`
  * `Information`: Crash is created and its location
  * `Warning`: If the crash dump directory couldn't be properly cleaned. This
    indicates a problem with the user account, which shouldn't occur.
  * `Error`: Crash dump file couldn't be created (or the ZIP file can't be
    generated).

#### Watchdog Logging

See the [developer watchdog](dev-watchdog.md) document for specific details.
