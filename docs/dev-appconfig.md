# CrashReporter Application Configuration File Format

This document covers a quick overview of the configuration file format for
reference. For information on modifying the `app.config` file for tracing, refer
to [Tracing](dev-tracing.md).

It applies to .NET Core as well as .NET Framework.

```xml
  <configSections>
    <sectionGroup name="CrashReporter">
      <section name="CrashDumper" type="RJCP.Diagnostics.Config.CrashDumper, RJCP.Diagnostics.CrashReporter"/>
      <section name="XmlCrashDumper" type="RJCP.Diagnostics.Config.XmlCrashDumper, RJCP.Diagnostics.CrashReporter"/>
      <section name="Watchdog" type="RJCP.Diagnostics.Config.Watchdog, RJCP.Diagnostics.CrashReporter"/>
    </sectionGroup>
  </configSections>
```

## Crash Dumper

Allows management where crash dumps are generated and some properties to manage
the memory size. See [Crash Dump Config](dev-crashdumpconfig.md).

```xml
  <CrashReporter>
    <CrashDumper>
      <DumpDirectory path="${CWD}/CrashDumps" ageDays="45" maxLogs="40"
        freeGb="5" freePercent="1" maxGb="1" minLogs="5"/>
    </CrashDumper>
  </CrashReporter>
```

The field `DumpDirectory` allows configures when dump files should be removed:
when

* the dump is older than `ageDays`; or
* the number of logs exceeds `maxLogs`; or
* there is less than `freeGb` disk space on the drive; or
* there is less than `freePercent` disk space on the drive; or
* the path given exceeds `maxGb` diskspace and there is more than `minLogs` on
  the drive.

## XML Crash Dumper

Refer to [XML Crash Dump](dev-xmlcrashdump.md) for detailed information.

```xml
  <CrashReporter>
    <XmlCrashDumper>
      <StyleSheet name="RJCP.Diagnostics.CrashReporter, RJCP.Diagnostics.CrashExport.Xml.CrashDump.xsl"/>
    </XmlCrashDumper>
  </CrashReporter>
```

This section allows you to:

* Specify a custom XSL to copy along side a generated XML file containing the
  crash dump. This can then be loaded using most browser tools. Note that most
  browsers of today don't load XML files with XSL transforms unless served via a
  webserver to avoid possible security holes.

## Watchdog

Refer to [Watchdog](dev-watchdog.md) for detailed information

```xml
  <CrashReporter>
    <Watchdog>
      <Ping stack="true"/>
      <Overrides>
        <Task name="app" warning="2000" critical="5000"/>
      </Overrides>
    </Watchdog>
  </CrashReporter>
```

This section allows you to:

* Override programmatic watchdog timeouts, as well as disable watchdog timeouts.
* Disable the automatic recording of stack trace information on a watchdog ping
  (default is enabled).
