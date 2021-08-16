# Configuration of Crash Dump

An applications behaviour for when `CreateDump` is called can be managed through
the application configuration section `CrashDumper`.

This document applies to .NET Framework as well as .NET Core.

```xml
  <configSections>
    <sectionGroup name="CrashReporter">
      <section name="CrashDumper" type="RJCP.Diagnostics.Config.CrashDumper, RJCP.Diagnostics.CrashReporter"/>
    </sectionGroup>
  </configSections>

  <CrashReporter>
    <CrashDumper>
      <DumpDirectory path="${LOCALAPPDIR}/CrashDumps" ageDays="45" maxLogs="40"
        freeGb="5" freePercent="1" maxGb="1" minLogs="5"/>
    </CrashDumper>
  </CrashReporter>
```

It is possible to change the default location for the application where crash
dumps are recorded and the disk space policy for the crash dumps. For example, a
command line application may wish to automatically record crash dumps in a
location relative to the program application, or the users current directory.

## Specifying a New Path

The property `path` allows to specify a new path. Paths may be absolute,
relative and may contain environment variables. Path directory specifiers are
generally done using the forward path specifier.

This path is used when the following methods are called:

* `Crash.Data.GetCrashDir(string prefix)`;
* `Crash.Data.Dump()`;
* `CrashReporter.CreateDump([CoreType coreType])`; and
* `CrashReporter.CreateDump(string fileName[, CoreType coreType)])` when
  `filename` is `null`.

### Environment Variables and Special Variables

To dynamically set the path at run-time based on the environment which the
application runs, the path can contain strings of the form `${VARIABLE}`. The
string `VARIABLE` is case sensitive and is resolved to be the value of an
environment variable of the program when it is started.

Further, there are some special environment variables that allow for a string to
work on multiple host operating systems:

* `LOCALAPPDATA` would normally resolve to `%LOCALAPPDATA%` on Windows, but uses
  `Environment.LocalApplicationData` so that a path can be found on Linux
  environments;
* `APPDATA` would normally resolve to `%APPDATA%` on Windows, but uses
  `Environment.ApplicationData` so that a path can be found on Linux
  environments;
* `HOME` resolves to the home directory `%USERPROFILE%` on Windows and the
  `HOME` environment variable on Linux.
* `CWD` maps the the current directory of the program (at the time the
  configuration is read in the program).
* `APPDIR` resolves the the location where the program was started.

## Specifying the Clean Up Policy

Over time, the crash directory path will increase in size. There is a policy
that will remove older crash dump files from disk so that the users hard disk
does not unnecessarily fill up.

The following properties can be used to manage the size of a crash dump
directory. These properties are calculated by each individual application. An
application with a more conservative setting for a specific path will apply the
policy when that application crashes. So for example, if application A has an
age of 10 days, application B has an age of 45 days, only files of 10 days and
newer are kept in the case when the more conservative application cleans up the
dump directory.

* `ageDays` specifies the age, in days, of the oldest crash dump. The age is
  given by the create date of the crash dump, not of any implicate date of the
  crash dump that may be embedded into the file name.
* `maxLogs` specifies the maximum number of logs to maintain in the crash dump
  directory.
* `freeGb` and `freePercent` are used with each other to determine how much
  reserve space on the current drive where logs are being stored should be
  maintained. Logs are removed until there is enough diskspace, given by
  `freeGb` (the number of GB that should be kept as reserve on the drive), or
  `freePercent` which is a percentage (value from 0 to 100) of the disk total
  space. So if the disk is 1TB and `freePercent` is 1, than the amount of free
  space is expected to be 10GB, or `freeGb` whichever is larger. Note, a reserve
  amount of space is always required when generating a log. The uncompressed
  data must first be written to disk before it is compressed.
* `maxGb` specifies the maximum size of the log directory. If the maximum size
  is exceeded, the oldest logs are removed, until the maximum size is under
  `maxGb` or there `minFiles` or less logs in the directory (so that not all
  logs are immediately removed as they may be required).
