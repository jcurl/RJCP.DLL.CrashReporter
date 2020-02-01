# XML Crash Dump Format

The default format is the XML Crash Dump (it is currently the only format
implemented). When a crash dump is created, it generates two files:

* The crash dump in XML format; and
* An XSL file that can be used to convert the XML to readable HTML.

## Providing Your Own XSL File

You might need your own XSL file instead of the built in one, for at least one
of the following reasons:

* The XSL has CSS embedded, but you don't like the colors; or
* You've provided your own CrashProvider and need special XSL transforms that
  knows how to display the information your custom CrashProvider classes
  generate; or
* etc.

To provide your own CSS you'll need to embed the CSS as a resource in your
application. To do this in Visual Studio:

* Add the XSL file to your project;
* Set the `Build Action` to `Embedded Resource`.

When your software is compiled, an embedded resource will be created depending
on where it is in your project.

For example, you have the default namespace of your application to be `MyApp`.
Then the XSL file is placed in your project into the folder
`MyCSS/CrashDump.xsl`. The embedded resource will then be named
`MyApp.MyCss.CrashDump.xsl`.

You can open your library in many tools to investigate the resources in
applications (e.g. ILSpy, dnSpy), or you can even write a small routine using
the API `assembly.GetManifestResourceNames()` to list the embedded resources and
their precise names.

Modify the `App.Config` to specify the location of the embedded resource.

`app.Config`

```xml
  <configSections>
    <sectionGroup name="CrashReporter">
      <section name="XmlCrashDumper" type="RJCP.Diagnostics.Config.XmlCrashDumper, RJCP.Diagnostics.CrashReporter"/>
    </sectionGroup>
  </configSections>

  <CrashReporter>
    <XmlCrashDumper>
      <StyleSheet name="RJCP.Diagnostics.CrashReporter, RJCP.Diagnostics.CrashExport.Xml.CrashDump.xsl"/>
    </XmlCrashDumper>
  </CrashReporter>
```

In the above example, we see that the XSL file is in the assembly
`RJCP.Diagnostics.CrashReporter` and the name of the resource is found at
`RJCP.Diagnostics.CrashExport.Xml>CrashDump.xsl` within that assembly. The
example provided is the default laoded in the case that no configuration is
given.

### The StyleSheet parameter

In more detail, there is only one attribute for the element `<StyleSheet>`,
called `name`. The string is written with two parts:

```
<assemblyName>, <resourceName>
```

If you do not provide the `<assemblyName>` part, then it is assumed that the
application (the entry assembly in the more general case) is the assembly which
should be used to search for the resource.

In case the resource could not be found, the default (built in) XSL is used.

When searching for the assembly to get the resource, only the loaded assemblies
are used - it won't try to load an assembly (like a resource assembly)
automatically, you, the programmer, must do this explicitly in code.
