<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="html" doctype-system="about:legacy-compat" encoding="UTF-8" indent="yes"/>

  <xsl:variable name="nl" select="'&#10;'"/>
  
  <xsl:template match="/DiagnosticDump">
    <xsl:comment>
    To get FireFox > 68 to load your XML file referencing this stylesheet:
     1. enter the URI: "about:config"
     2. Search for the parameter: "privacy.file_unique_origin"
     3. Change the value from "true" to "false"
     4. Drag and drop the XML file referencing this style sheet to FireFox
    </xsl:comment>

    <html>
      <head>
        <title>Crash Dump Log</title>
        <style>
          body {
            background-color:#d4d5e2;
            font-family: "Trebuchet MS", Helvetica, sans-serif;
            font-size: 0.75em;
          }
          h1 {
            background-color:#013a58;
            color:white;
          }
          h2 {
            background-color:#013a58;
            color:white;
          }
          h3 {
            background-color:#235c7a;
            color:white;
          }
          table, th, td {
            border: 1px solid black;
            border-collapse: collapse;
            font-size: 1em;
          }
          th {
            text-align: left;
            background-color:#9fa4b2;
            color:black;
          }
          td {
            background-color:#bfc8d5;
            color:black;
          }
          td.timestamp {
            white-space:nowrap;
          }
          pre {
            font-family: "Trebuchet MS", Helvetica, sans-serif;
            overflow-x: auto;
            white-space: pre-wrap;
            white-space: -moz-pre-wrap;
            white-space: -pre-wrap;
            white-space: -o-pre-wrap;
            word-wrap: break-word;
          }
          .pass {
            background-color: #a0f0c0;
            color: #000000;
          }
          .fail {
            background-color: #f0a0c0;
            color: #ffffff;
          }
          .eventVerbose {
            background-color:#bfc8d5;
          }
          .eventInformation {
            background-color:#bfc8d5;
          }
          .eventWarning {
            background-color: #f0f0c0;
          }
          .eventError {
            background-color: #f0a0c0;
          }
          .eventCritical {
            background-color: #f0a0c0;
          }
        </style>
      </head>
      <body>
        <h1>System Diagnostics</h1>
        <xsl:apply-templates/>
      </body>
    </html>
  </xsl:template>

  <xsl:template match="NetVersionInstalled">
    <h2>.NET Version Installed</h2>
    <xsl:call-template name="ItemNetInstalled"/>
  </xsl:template>

  <xsl:template match="NetVersionRunning">
    <h2>.NET Version Running</h2>
    <xsl:call-template name="ItemNetRunning"/>
  </xsl:template>

  <xsl:template match="Assemblies">
    <h2>Loaded Assemblies</h2>
    <xsl:call-template name="ItemAssemblies"/>
  </xsl:template>

  <xsl:template match="EnvironmentVariables">
    <h2>Environment Variables</h2>
    <xsl:call-template name="ItemNameValueList"/>
  </xsl:template>

  <xsl:template match="Network">
    <h2>Network Interfaces</h2>
    <xsl:call-template name="ItemNetwork"/>
  </xsl:template>

  <xsl:template match="OSThreads">
    <h2>OS Threads</h2>
    <xsl:call-template name="ItemOSThreads"/>
  </xsl:template>

  <xsl:template match="OSInfo">
    <h2>OS Information</h2>
    <xsl:call-template name="ItemPropertyValueList"/>
  </xsl:template>

  <xsl:template match="WinOSInfo">
    <h2>OS Information (Windows)</h2>
    <xsl:call-template name="ItemPropertyValueList"/>
  </xsl:template>

  <xsl:template match="ProcessInfo">
    <h2>Process Information</h2>
    <xsl:call-template name="ItemPropertyValueList"/>
  </xsl:template>

  <xsl:template match="Modules">
    <h2>Loaded Modules (DLLs)</h2>
    <xsl:call-template name="ItemModules"/>
  </xsl:template>

  <xsl:template match="TraceListenerLog">
    <h2>Log</h2>
    <xsl:call-template name="LogEntries"/>
  </xsl:template>

  <xsl:template name="ItemNetInstalled">
    <table width="100%">
      <tr>
        <th width="20%">.NET Version</th>
        <th width="80%">Description</th>
      </tr>
      <xsl:for-each select="item">
        <tr>
          <td><xsl:value-of select="@Installed"/></td>
          <td><xsl:value-of select="@Description"/></td>
        </tr>
      </xsl:for-each>
    </table>
  </xsl:template>

  <xsl:template name="ItemNetRunning">
    <table width="100%">
      <tr>
        <th width="20%">.NET Version</th>
        <th width="80%">Description</th>
      </tr>
      <xsl:for-each select="item">
        <tr>
          <td><xsl:value-of select="@Running"/> (<xsl:value-of select="@Version"/>)</td>
          <td><xsl:value-of select="@Description"/></td>
        </tr>
      </xsl:for-each>
    </table>
  </xsl:template>

  <xsl:template name="ItemAssemblies">
    <table width="100%">
      <tr>
        <th width="20%">Assembly</th>
        <th width="80%" colspan="2">Full Name</th>
      </tr>
      <xsl:for-each select="assembly">
        <xsl:sort select="@name"/>
        <tr>
          <td rowspan="7" valign="top"><xsl:value-of select="@name"/></td>
          <td colspan="2"><xsl:value-of select="@fullname"/></td>
        </tr>
        <tr>
          <td width="10%">Version:</td>
          <td><xsl:value-of select="@version"/></td>
        </tr>
        <tr>
          <td width="10%">Version Info:</td>
          <td><xsl:value-of select="@versioninfo"/></td>
        </tr>
        <tr>
          <td width="10%">Version File:</td>
          <td><xsl:value-of select="@versionfile"/></td>
        </tr>
        <tr>
          <td width="10%">Processor:</td>
          <td><xsl:value-of select="@processor"/></td>
        </tr>
        <tr>
          <td width="10%">Location:</td>
          <td><xsl:value-of select="@location"/></td>
        </tr>
        <tr>
          <td width="10%">Code Base:</td>
          <td><xsl:value-of select="@codebase"/></td>
        </tr>
      </xsl:for-each>
    </table>
  </xsl:template>

  <xsl:template name="ItemPropertyValueList">
    <table width="100%">
      <tr>
        <th width="20%">Property</th>
        <th width="80%">Value</th>
      </tr>
      <xsl:for-each select="item">
        <tr>
          <td><xsl:value-of select="@property"/></td>
          <td><xsl:value-of select="@value"/></td>
        </tr>
      </xsl:for-each>
    </table>
  </xsl:template>

  <xsl:template name="ItemNameValueList">
    <table width="100%">
      <tr>
        <th width="20%">Property</th>
        <th width="80%">Value</th>
      </tr>
      <xsl:for-each select="item">
        <tr>
          <td><xsl:value-of select="@name"/></td>
          <td><xsl:value-of select="@value"/></td>
        </tr>
      </xsl:for-each>
    </table>
  </xsl:template>

  <xsl:template name="ItemNetwork">
    <table width="100%">
      <tr>
        <th width="20%">Interface</th>
        <th width="80%" colspan="2">Description</th>
      </tr>
      <xsl:for-each select="item">
        <xsl:sort select="@name"/>
        <tr>
          <td rowspan="16" valign="top"><xsl:value-of select="@name"/></td>
          <td colspan="2"><xsl:value-of select="@description"/></td>
        </tr>
        <tr>
          <td width="10%">Identifier:</td>
          <td><xsl:value-of select="@id"/></td>
        </tr>
        <tr>
          <td width="10%">Type:</td>
          <td><xsl:value-of select="@type"/></td>
        </tr>
        <tr>
          <td width="10%">Status:</td>
          <td><xsl:value-of select="@status"/></td>
        </tr>
        <tr>
          <td width="10%">Speed:</td>
          <td><xsl:value-of select="@speed"/></td>
        </tr>
        <tr>
          <td width="10%">MAC Address:</td>
          <td><xsl:value-of select="@mac"/></td>
        </tr>
        <tr>
          <td width="10%">DNS:</td>
          <td><xsl:value-of select="@dnsenabled"/></td>
        </tr>
        <tr>
          <td width="10%">DNS Suffix:</td>
          <td><xsl:value-of select="@dnssuffix"/></td>
        </tr>
        <tr>
          <td width="10%">DNS Dynamic:</td>
          <td><xsl:value-of select="@dnsdynenabled"/></td>
        </tr>
        <tr>
          <td width="10%">DNS Addresses:</td>
          <td><xsl:value-of select="@dnsaddr"/></td>
        </tr>
        <tr>
          <td width="10%">DHCP Addresses:</td>
          <td><xsl:value-of select="@dhcpaddr"/></td>
        </tr>
        <tr>
          <td width="10%">IP Addresses:</td>
          <td><xsl:value-of select="@ipaddr"/></td>
        </tr>
        <tr>
          <td width="10%">Gateway Addresses:</td>
          <td><xsl:value-of select="@gwaddr"/></td>
        </tr>
        <tr>
          <td width="10%">Anycast Addresses:</td>
          <td><xsl:value-of select="@anycastaddr"/></td>
        </tr>
        <tr>
          <td width="10%">Multicast:</td>
          <td><xsl:value-of select="@multicast"/></td>
        </tr>
        <tr>
          <td width="10%">Multicast Addresses:</td>
          <td><xsl:value-of select="@multicastaddr"/></td>
        </tr>
      </xsl:for-each>
    </table>
  </xsl:template>

  <xsl:template name="ItemOSThreads">
    <table width="100%">
      <tr>
        <th width="20%">ID</th>
        <th width="20%">State</th>
        <th width="30%">Priority</th>
        <th width="30%">Time (seconds)</th>
      </tr>
      <xsl:for-each select="thread">
        <xsl:sort select="@id" data-type="number"/>
        <tr>
          <td><xsl:value-of select="@id"/></td>
          <td><xsl:value-of select="@state"/></td>
          <td>Base=<xsl:value-of select="@basePrio"/>; Prio=<xsl:value-of select="@prio"/></td>
          <td>Total=<xsl:value-of select="@totalTime"/>; User=<xsl:value-of select="@userTime"/><xsl:value-of select="@id"/></td>
        </tr>
      </xsl:for-each>
    </table>
  </xsl:template>

  <xsl:template name="ItemModules">
    <table width="100%">
      <tr>
        <th width="20%">Module</th>
        <th width="80%" colspan="2">Description</th>
      </tr>
      <xsl:for-each select="module">
        <xsl:sort select="@name"/>
        <tr>
          <td rowspan="7" valign="top"><xsl:value-of select="@name"/></td>
          <td colspan="2"><xsl:value-of select="@fileDesc"/></td>
        </tr>
        <tr>
          <td width="10%">File Name:</td>
          <td><xsl:value-of select="@originalFileName"/>; <xsl:value-of select="@fileName"/></td>
        </tr>
        <tr>
          <td width="10%">File Version:</td>
          <td><xsl:value-of select="@fileVersion"/></td>
        </tr>
        <tr>
          <td width="10%">Product Name:</td>
          <td><xsl:value-of select="@productName"/></td>
        </tr>
        <tr>
          <td width="10%">Product Version:</td>
          <td><xsl:value-of select="@productVersion"/></td>
        </tr>
        <tr>
          <td width="10%">Base Address:</td>
          <td>
            0x<xsl:call-template name="decimalToHex">
              <xsl:with-param name="dec">
                <xsl:value-of select="@baseAddress"/>
              </xsl:with-param>
            </xsl:call-template>
          </td>
        </tr>
        <tr>
          <td width="10%">Memory Size:</td>
          <td>
            0x<xsl:call-template name="decimalToHex">
              <xsl:with-param name="dec">
                <xsl:value-of select="@memorySize"/>
              </xsl:with-param>
            </xsl:call-template>
          </td>
        </tr>
      </xsl:for-each>
    </table>
  </xsl:template>

  <xsl:template name="decimalToHex">
    <xsl:param name="dec"/>
    <xsl:if test="$dec > 0">
      <xsl:call-template name="decimalToHex">
        <xsl:with-param name="dec" select="floor($dec div 16)"/>
      </xsl:call-template>
      <xsl:value-of select="substring('0123456789ABCDEF', (($dec mod 16) + 1), 1)"/>
    </xsl:if>
  </xsl:template>

  <xsl:template name="LogEntries">
    <xsl:variable name="break">&#13;</xsl:variable>
    <table width="100%">
      <tr>
        <th>Time Stamp</th>
        <th>Event Type</th>
        <th>Source</th>
        <th>ID</th>
        <th>Thread</th>
        <th>Message</th>
      </tr>
      <xsl:for-each select="entry">
        <tr>
          <td class="timestamp"><xsl:value-of select="@timestamp"/> (<xsl:value-of select="@clock"/>)</td>
          <td><xsl:value-of select="@eventType"/></td>
          <td><xsl:value-of select="@source"/></td>
          <td><xsl:value-of select="@id"/></td>
          <td><xsl:value-of select="@threadid"/></td>
          <td>
            <pre>
              <xsl:value-of select="@message" disable-output-escaping="no"/>
            </pre>
          </td>
        </tr>
      </xsl:for-each>
    </table>
  </xsl:template>
</xsl:stylesheet>
