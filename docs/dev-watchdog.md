# Using Watchdog Functionality

The library `RJCP.Diagnostics.CrashReporter.dll` provides functionality allowing
you to monitor your threads. A watchdog of a specific name is registered with a
warning and a critical timeout, and then threads are expected to "ping" the
watchdog to reset the timer. If threads don't ping the watchdog within the
warning timeout, a crash dump is automatically generated (but no core). If
threads don't ping the watchdog within the critical timeout, a crash dump and a
core is generated (as configured in `CrashReporter`) and the program is exited.

## Why Use Watchdog Functionality

Multithreaded programs are difficult to write, and even with the .NET
async/await model, it is still easy to enter deadlocks in an application
(through incorrect use of locks, or a `Task` incorrectly never has a condition
set due to programmatic race conditions).

In case that a dead lock occurs, the user normally sees the program hang, and
they try to close it. One might be lucky if the user is technical enough to be
able to create a core dump.

The Watchdog functionality here will detect a timeout that the programmer
implements, and do a crash dump. Default behaviour is to automatically generate
crash information on watchdog warnings and core dumps on watchdog errors, so
that the user can provide this to the developer to improve software.

## Programming the Watchdog

### Registering a Watchdog

Watchdogs are all given a string name. Each name must be unique in the software.
Normally, code that sets up the threads, or tasks that run over a longer period
of time would register a watchdog.

```csharp
using RJCP.Diagnostics;

int warning = 30000;
int critical = 45000;
CrashReporter.Watchdog.Register("operationname", warning, critical);
```

The code above registers the watchdog with the name `operationname` with a
warning generated after 30s, and the program would exit after 45s. The watchdog
timer is started as soon as it is registered.

### Feeding the Watchdog

Once the watchdog is set up, it must be pinged at a frequency of at least the
`warning` timeout. Usually it is best to design the software so that pinging the
watchdog is about twice as fast as the warning timeout, and that the warning
timeout is about two thirds (2/3) of the critical timeout.

```csharp
using RJCP.Diagnostics;

CrashReporter.Watchdog.Ping("operationname");
```

### Unregistering the Watchdog

When the operation is finished, the watchdog should be unregistered so that
there are no unintended timeouts resulting in a crash.

```csharp
using RJCP.Diagnostics;

CrashReporter.Watchdog.Unregister("operationname");
```

## Logging

### Setting up the App.Config File

All information about the watchdog is logged to the `LogSource` category called
`RJCP.CrashReporter.Watchdog`. This trace source should normally be assigned the
same logging as your application, e.g. to the `SimplePrioMemoryTraceListener`.

For .NET Framework 4.x, this is done using the standard `TraceSource` logic
present in the framework. For .NET Core the `.AddSimplePrioMemoryLogger()`
extension registers the trace in the Host, and not as part of the `app.config`
file (it isn't read). See [dev-tracing](dev-tracing.md) for more information.

`app.config` for .NET Framework

### Trace Levels and Sources

#### TraceEventType.Error

Everything logged at this level includes:

- `RJCP.CrashReporter.Watchdog`: Critical watchdog timeouts. Information about
  the name, the timeout that was configured, when and where it was registered,
  the last ping (and where the last ping occurred) and the current thread are
  logged.
- `RJCP.CrashReporter` (or the category defined by `CrashReporter.Source`): If
  there was an error creating a dump on a critical watchdog timeout.

#### TraceEventType.Warning

Everything logged at this level includes:

- `RJCP.CrashReporter.Watchdog`
  - Registering a watchdog of the same name twice. The first watchdog is
    registered, but the second registration is ignored. Information about the
    name, when and were it was registered and the current callstack is logged.
  - Unregistering a watchdog that was previously unregistered, or has never been
    registered. Information about the name and the callstack is logged.
  - Pinging a watchdog that was previously unregistered, or has never been
    registered. Information about the name and the callstack is logged.
  - A watchdog warning has occurred. Every watchdog that has expired is only
    logged once, pinging the watchdog reactivates the watchdog timeout again.
    Information about the name of the watchdog, the warning timeout, when and
    where it was registered, and the time and location of the last ping, as well
    as the current managed thread.
- `RJCP.CrashReporter` (or the category defined by `CrashReporter.Source`)
  - If a watchdog critical dump could not be created, it is logged.

#### TraceEventType.Information

Everything logged at this level includes:

- `RJCP.CrashReporter.Watchdog`
  - First time registration of a watchdog. The name, warning timeout and
    critical timeout are logged.
  - Unregistering a previously registered watchdog. The name of the watchdog is
    logged.
- `RJCP.CrashReporter` (or the category defined by `CrashReporter.Source`)
  - If a watchdog warning could not be created, it is logged.
  - The location of a watchdog timeout (critical time expired).

#### TraceEventType.Verbose

Everything logged at this level includes:

- `RJCP.CrashReporter.Watchdog`
  - When pinging a watchdog. The name of the watchdog is logged.
- `RJCP.CrashReporter` (or the category defined by `CrashReporter.Source`)
  - Logs the location of a watchdog warning log.

## Advanced Techniques

### Handling a Warning Event

Provide an event to the `CrashReporter.Watchdog.WarningEvent` to override the
default behaviour when a watchdog warning occurs. If the event is assigned, then
this overrides default behaviour and no crash dumps would occur. The
implementation of your own `WarningEvent` must do this programmatically.

To obtain default behaviour, you can call `ThreadWatchdog.DefaultWarningEvent`.

You might want to override this functionality to programmatically warn the user,
or collect extra information specific to your program.

The event provide a list of watchdog items that can be used to know what
watchdogs expired.

### Handling a Critical Timeout Event

Similar to the warnings, provide an event to the
`CrashReporter.Watchdog.CriticalEvent`. When providing the event, this overrides
the default behaviour, so that no crash dumps will occur and the program will
not exit.

To obtain default behaviour, you can call `ThreadWatchdog.DefaultCriticalEvent`.

You might want to override this functionality programmatically if you do not
wish to end the program abruptly, or to save state to allow the user to
reconvene their work.

The event provide a list of watchdog items that can be used to know what
watchdogs expired.

### Providing other Timer Sources

One reason to provide other timer sources is to change behaviour of clocks and
how those clocks are monitored. One may have special requirements on accuracy,
or require the events which trigger the watchdog timeouts to occur under
specific circumstances (e.g. a dedicated thread).

When the `ThreadWatchdog` is instantiated, it uses by default the
`MonotonicTimerSource` which obtains the time from `Environment.TickCount` as a
32-bit signed integer, and uses `SingleShotTimer` to handle timing events which
uses the `System.Threading.Thread.Timer` object to get notification when a
watchdog timeout occurs.

Providing other timer sources may be useful for unit testing.  For example, in
the test cases, there are the classes `VirtualTimerSource` which allows the user
to programmatically increment the clock, and the `VirtualSingleShotTimer` which
takes the same `VirtualTimerSource` to raise events when the virtual timer
source is updated.

This allows precise control over when watchdog events can occur, by stating
precisely how much time is incremented. When the
`VirtualTimerSource.UpdateClock(int)` is called, events occur on that thread
instead of a pool thread. See the unit test cases on how to use the virtual
timers for manipulating the watchdog timeouts.

## Overriding Watchdog Behaviour

### Adjusting the Timeout Values

It is possible to modify the configuration file of the application to override
the warning and critical timeout values. This is useful in situations where
software may use a timeout that is too conservative, and as a short-term
workaround, the watchdog values might need to be relaxed.

In a similar manner, the timeouts can be shortened, which can be useful to test
watchdog behaviour.

If there is no configuration for the CrashReporter at all in your software, add
a section similar to the example below.

`app.config`

```xml
  <configSections>
    <sectionGroup name="CrashReporter">
      <section name="Watchdog" type="RJCP.Diagnostics.Config.Watchdog, RJCP.Diagnostics.CrashReporter"/>
    </sectionGroup>
  </configSections>

  <CrashReporter>
    <Watchdog>
      <Overrides>
        <Task name="app" warning="30000" critical="-1"/>
      </Overrides>
    </Watchdog>
  </CrashReporter>
```

In the `<Overrides>` section, add an override, with the name of the watchdog
task (to determine the application names, look at the code base, or the log
files) and the timeout values that should be used.

All timeout values are in milliseconds. A timeout of -1 is to disable that
particular watchdog. So if a warning is still required, but the program should
not exit, the `critical` can be set to -1 as given in the example above.

If using the `XmlCrashDumper` in the configuration, merge the XML elements
together, such as:

`app.config`

```xml
  <configSections>
    <sectionGroup name="CrashReporter">
      <section name="XmlCrashDumper" type="RJCP.Diagnostics.Config.XmlCrashDumper, RJCP.Diagnostics.CrashReporter"/>
      <section name="Watchdog" type="RJCP.Diagnostics.Config.Watchdog, RJCP.Diagnostics.CrashReporter"/>
    </sectionGroup>
  </configSections>

  <CrashReporter>
    <XmlCrashDumper>
      <StyleSheet name="RJCP.Diagnostics.CrashReporter, RJCP.Diagnostics.CrashExport.Xml.CrashDump.xsl"/>
    </XmlCrashDumper>
    <Watchdog>
      <Overrides>
        <Task name="app" warning="30000" critical="-1"/>
      </Overrides>
    </Watchdog>
  </CrashReporter>
```

### Disabling the Stack Capture on Ping

The watchdog functionality will capture the stack when an application is
registered, and when a ping occurs. This assists with debugging should a
watchdog warning or timeout occur in determining when the last ping occurred,
and where the watchdog was first registered.

One can disable the functionality to capture the stack on ping by adding the
following in the applicaton configuration file.

`app.config`

```xml
  <CrashReporter>
    <Watchdog>
      <Ping stack="false"/>
    </Watchdog>
  </CrashReporter>
```

The default behaviour is to enable the stack capture. Set the attribute `stack`
to `false` to turn off the stack capture feature on ping. No stack is captured
which makes the ping operation faster.

The programmer can disable the stack feature always. If this is done, the
application configuration section has no effect and stack capture on a ping is
always disabled. This would mean though it is not possible to enable the feature
with out recompiling if behaviour should change.

```csharp
CrashReporter.Config.Watchdog.Ping.StackCapture = false;
```

Setting this value back to `true` enables the stack capture if not disabled in
the configuration file.
