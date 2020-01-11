# Tracing using Crash Reporter

## Why use Crash Reporter

.NET provides trace functionality, but the default `TraceListeners` log to the
console always, or log to disk always. They append logs as required. Most of the
time these logs are not required, can slow down your application slightly while
writing to disk and more importantly, fill up the space on the users harddisk,
pushing the onus onto the user to remove unneeded trace files, or to disable
traces (and so have no possibility to report issues without reenabling and
spending significant effort in reproduction).

The Crash Reporter traces in RAM initially, uses a prioritized queue to discard
old traces as RAM usage increases, and only writes them to disk when instructed
to do so. This means traces are only saved as they're required (when there's a
problem).

## Quick Guide

Reference the `RJCP.Diagnostics.CrashReporter` library into your project, and
modify the `App.config` file to include the following:

```xml
  <system.diagnostics>
    <sharedListeners>
      <add name="myListener" type="RJCP.Diagnostics.Trace.SimplePrioMemoryTraceListener, RJCP.Diagnostics.CrashReporter"/>
    </sharedListeners>

    <sources>
      <source name="CrashReporterApp" switchValue="Verbose">
        <listeners>
          <remove name="Default"/>
          <add name="myListener"/>
        </listeners>
      </source>
    </sources>
    <trace autoflush="true" useGlobalLock="false"/>
  </system.diagnostics>
```

This will capture all traces internally in RAM, and only output them to disk
when instructed to by your program.

```csharp
using System;
using System.IO;
using RJCP.Diagnostics;
using RJCP.Diagnostics.Dump;

namespace CrashReportApp
{
    static class Program
    {
        static void Main()
        {
            CrashReporter.Source = Log.App;
            CrashReporter.SetExceptionHandlers();

            Log.App.TraceEvent(System.Diagnostics.TraceEventType.Information, 0, "Program Started");

            // Dump all traces (no core dump) and information to the current directory
            string path = Path.Combine(Environment.CurrentDirectory, Crash.Data.CrashDumpFactory.FileName);
            Crash.Data.Dump(path);

            // Now simulate an unhandled exception, which creates a core dump
            throw new InvalidOperationException("An exception which should cause a dump");
        }
    }
}
```

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
      <add name="myListener" type="RJCP.Diagnostics.Trace.SimplePrioMemoryTraceListener, RJCP.Diagnostics.CrashReporter" Critical="100" Error="150" Warning="200" Information="250" Verbose="300" Other="100" Total="1500"/>
    </sharedListeners>
```

The attributes defined in the configuration file define the size of the queues.
If the Total is less than the sum of all the other queues, it will be implicitly
be set to the minimum required to accomodate all the queues. If it is larger,
then messages will be discarded later.

### Logging Multiple Trace Sources into a Single Listener

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