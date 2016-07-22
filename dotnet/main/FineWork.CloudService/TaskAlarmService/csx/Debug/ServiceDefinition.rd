<?xml version="1.0" encoding="utf-8"?>
<serviceModel xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" name="taskalarmworker" generation="1" functional="0" release="0" Id="735b8e2a-e429-4c56-b98b-da7baabe26c5" dslVersion="1.2.0.0" xmlns="http://schemas.microsoft.com/dsltools/RDSM">
  <groups>
    <group name="taskalarmworkerGroup" generation="1" functional="0" release="0">
      <componentports>
        <inPort name="FwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput" protocol="tcp">
          <inToChannel>
            <lBChannelMoniker name="/taskalarmworker/taskalarmworkerGroup/LB:FwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput" />
          </inToChannel>
        </inPort>
      </componentports>
      <settings>
        <aCS name="Certificate|FwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" defaultValue="">
          <maps>
            <mapMoniker name="/taskalarmworker/taskalarmworkerGroup/MapCertificate|FwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" />
          </maps>
        </aCS>
        <aCS name="Certificate|FwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteDebugger.TransportValidation" defaultValue="">
          <maps>
            <mapMoniker name="/taskalarmworker/taskalarmworkerGroup/MapCertificate|FwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteDebugger.TransportValidation" />
          </maps>
        </aCS>
        <aCS name="FwWorkerRole:CloudToolsDiagnosticAgentVersion" defaultValue="">
          <maps>
            <mapMoniker name="/taskalarmworker/taskalarmworkerGroup/MapFwWorkerRole:CloudToolsDiagnosticAgentVersion" />
          </maps>
        </aCS>
        <aCS name="FwWorkerRole:ConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/taskalarmworker/taskalarmworkerGroup/MapFwWorkerRole:ConnectionString" />
          </maps>
        </aCS>
        <aCS name="FwWorkerRole:IntelliTrace.IntelliTraceConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/taskalarmworker/taskalarmworkerGroup/MapFwWorkerRole:IntelliTrace.IntelliTraceConnectionString" />
          </maps>
        </aCS>
        <aCS name="FwWorkerRole:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/taskalarmworker/taskalarmworkerGroup/MapFwWorkerRole:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </maps>
        </aCS>
        <aCS name="FwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword" defaultValue="">
          <maps>
            <mapMoniker name="/taskalarmworker/taskalarmworkerGroup/MapFwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword" />
          </maps>
        </aCS>
        <aCS name="FwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration" defaultValue="">
          <maps>
            <mapMoniker name="/taskalarmworker/taskalarmworkerGroup/MapFwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration" />
          </maps>
        </aCS>
        <aCS name="FwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername" defaultValue="">
          <maps>
            <mapMoniker name="/taskalarmworker/taskalarmworkerGroup/MapFwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername" />
          </maps>
        </aCS>
        <aCS name="FwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" defaultValue="">
          <maps>
            <mapMoniker name="/taskalarmworker/taskalarmworkerGroup/MapFwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" />
          </maps>
        </aCS>
        <aCS name="FwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteDebugger.ClientThumbprint" defaultValue="">
          <maps>
            <mapMoniker name="/taskalarmworker/taskalarmworkerGroup/MapFwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteDebugger.ClientThumbprint" />
          </maps>
        </aCS>
        <aCS name="FwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteDebugger.Connector.Enabled" defaultValue="">
          <maps>
            <mapMoniker name="/taskalarmworker/taskalarmworkerGroup/MapFwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteDebugger.Connector.Enabled" />
          </maps>
        </aCS>
        <aCS name="FwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteDebugger.Connector.Version" defaultValue="">
          <maps>
            <mapMoniker name="/taskalarmworker/taskalarmworkerGroup/MapFwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteDebugger.Connector.Version" />
          </maps>
        </aCS>
        <aCS name="FwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteDebugger.ServerThumbprint" defaultValue="">
          <maps>
            <mapMoniker name="/taskalarmworker/taskalarmworkerGroup/MapFwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteDebugger.ServerThumbprint" />
          </maps>
        </aCS>
        <aCS name="FwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled" defaultValue="">
          <maps>
            <mapMoniker name="/taskalarmworker/taskalarmworkerGroup/MapFwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled" />
          </maps>
        </aCS>
        <aCS name="FwWorkerRole:PushKey" defaultValue="">
          <maps>
            <mapMoniker name="/taskalarmworker/taskalarmworkerGroup/MapFwWorkerRole:PushKey" />
          </maps>
        </aCS>
        <aCS name="FwWorkerRole:PushSecret" defaultValue="">
          <maps>
            <mapMoniker name="/taskalarmworker/taskalarmworkerGroup/MapFwWorkerRole:PushSecret" />
          </maps>
        </aCS>
        <aCS name="FwWorkerRole:StorageConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/taskalarmworker/taskalarmworkerGroup/MapFwWorkerRole:StorageConnectionString" />
          </maps>
        </aCS>
        <aCS name="FwWorkerRoleInstances" defaultValue="[1,1,1]">
          <maps>
            <mapMoniker name="/taskalarmworker/taskalarmworkerGroup/MapFwWorkerRoleInstances" />
          </maps>
        </aCS>
      </settings>
      <channels>
        <sFSwitchChannel name="IE:FwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteDebugger.Connector">
          <toPorts>
            <inPortMoniker name="/taskalarmworker/taskalarmworkerGroup/FwWorkerRole/Microsoft.WindowsAzure.Plugins.RemoteDebugger.Connector" />
          </toPorts>
        </sFSwitchChannel>
        <sFSwitchChannel name="IE:FwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteDebugger.FileUpload">
          <toPorts>
            <inPortMoniker name="/taskalarmworker/taskalarmworkerGroup/FwWorkerRole/Microsoft.WindowsAzure.Plugins.RemoteDebugger.FileUpload" />
          </toPorts>
        </sFSwitchChannel>
        <sFSwitchChannel name="IE:FwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteDebugger.Forwarder">
          <toPorts>
            <inPortMoniker name="/taskalarmworker/taskalarmworkerGroup/FwWorkerRole/Microsoft.WindowsAzure.Plugins.RemoteDebugger.Forwarder" />
          </toPorts>
        </sFSwitchChannel>
        <lBChannel name="LB:FwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput">
          <toPorts>
            <inPortMoniker name="/taskalarmworker/taskalarmworkerGroup/FwWorkerRole/Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput" />
          </toPorts>
        </lBChannel>
        <sFSwitchChannel name="SW:FwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp">
          <toPorts>
            <inPortMoniker name="/taskalarmworker/taskalarmworkerGroup/FwWorkerRole/Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp" />
          </toPorts>
        </sFSwitchChannel>
      </channels>
      <maps>
        <map name="MapCertificate|FwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" kind="Identity">
          <certificate>
            <certificateMoniker name="/taskalarmworker/taskalarmworkerGroup/FwWorkerRole/Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" />
          </certificate>
        </map>
        <map name="MapCertificate|FwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteDebugger.TransportValidation" kind="Identity">
          <certificate>
            <certificateMoniker name="/taskalarmworker/taskalarmworkerGroup/FwWorkerRole/Microsoft.WindowsAzure.Plugins.RemoteDebugger.TransportValidation" />
          </certificate>
        </map>
        <map name="MapFwWorkerRole:CloudToolsDiagnosticAgentVersion" kind="Identity">
          <setting>
            <aCSMoniker name="/taskalarmworker/taskalarmworkerGroup/FwWorkerRole/CloudToolsDiagnosticAgentVersion" />
          </setting>
        </map>
        <map name="MapFwWorkerRole:ConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/taskalarmworker/taskalarmworkerGroup/FwWorkerRole/ConnectionString" />
          </setting>
        </map>
        <map name="MapFwWorkerRole:IntelliTrace.IntelliTraceConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/taskalarmworker/taskalarmworkerGroup/FwWorkerRole/IntelliTrace.IntelliTraceConnectionString" />
          </setting>
        </map>
        <map name="MapFwWorkerRole:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/taskalarmworker/taskalarmworkerGroup/FwWorkerRole/Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </setting>
        </map>
        <map name="MapFwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword" kind="Identity">
          <setting>
            <aCSMoniker name="/taskalarmworker/taskalarmworkerGroup/FwWorkerRole/Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword" />
          </setting>
        </map>
        <map name="MapFwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration" kind="Identity">
          <setting>
            <aCSMoniker name="/taskalarmworker/taskalarmworkerGroup/FwWorkerRole/Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration" />
          </setting>
        </map>
        <map name="MapFwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername" kind="Identity">
          <setting>
            <aCSMoniker name="/taskalarmworker/taskalarmworkerGroup/FwWorkerRole/Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername" />
          </setting>
        </map>
        <map name="MapFwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" kind="Identity">
          <setting>
            <aCSMoniker name="/taskalarmworker/taskalarmworkerGroup/FwWorkerRole/Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" />
          </setting>
        </map>
        <map name="MapFwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteDebugger.ClientThumbprint" kind="Identity">
          <setting>
            <aCSMoniker name="/taskalarmworker/taskalarmworkerGroup/FwWorkerRole/Microsoft.WindowsAzure.Plugins.RemoteDebugger.ClientThumbprint" />
          </setting>
        </map>
        <map name="MapFwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteDebugger.Connector.Enabled" kind="Identity">
          <setting>
            <aCSMoniker name="/taskalarmworker/taskalarmworkerGroup/FwWorkerRole/Microsoft.WindowsAzure.Plugins.RemoteDebugger.Connector.Enabled" />
          </setting>
        </map>
        <map name="MapFwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteDebugger.Connector.Version" kind="Identity">
          <setting>
            <aCSMoniker name="/taskalarmworker/taskalarmworkerGroup/FwWorkerRole/Microsoft.WindowsAzure.Plugins.RemoteDebugger.Connector.Version" />
          </setting>
        </map>
        <map name="MapFwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteDebugger.ServerThumbprint" kind="Identity">
          <setting>
            <aCSMoniker name="/taskalarmworker/taskalarmworkerGroup/FwWorkerRole/Microsoft.WindowsAzure.Plugins.RemoteDebugger.ServerThumbprint" />
          </setting>
        </map>
        <map name="MapFwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled" kind="Identity">
          <setting>
            <aCSMoniker name="/taskalarmworker/taskalarmworkerGroup/FwWorkerRole/Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled" />
          </setting>
        </map>
        <map name="MapFwWorkerRole:PushKey" kind="Identity">
          <setting>
            <aCSMoniker name="/taskalarmworker/taskalarmworkerGroup/FwWorkerRole/PushKey" />
          </setting>
        </map>
        <map name="MapFwWorkerRole:PushSecret" kind="Identity">
          <setting>
            <aCSMoniker name="/taskalarmworker/taskalarmworkerGroup/FwWorkerRole/PushSecret" />
          </setting>
        </map>
        <map name="MapFwWorkerRole:StorageConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/taskalarmworker/taskalarmworkerGroup/FwWorkerRole/StorageConnectionString" />
          </setting>
        </map>
        <map name="MapFwWorkerRoleInstances" kind="Identity">
          <setting>
            <sCSPolicyIDMoniker name="/taskalarmworker/taskalarmworkerGroup/FwWorkerRoleInstances" />
          </setting>
        </map>
      </maps>
      <components>
        <groupHascomponents>
          <role name="FwWorkerRole" generation="1" functional="0" release="0" software="D:\FineWork\devel\sources\dotnet\main\FineWork.CloudService\TaskAlarmService\csx\Debug\roles\FwWorkerRole" entryPoint="base\x64\WaHostBootstrapper.exe" parameters="base\x64\WaWorkerHost.exe " memIndex="-1" hostingEnvironment="consoleroleadmin" hostingEnvironmentVersion="2">
            <componentports>
              <inPort name="Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput" protocol="tcp" />
              <inPort name="Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp" protocol="tcp" portRanges="3389" />
              <outPort name="FwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp" protocol="tcp">
                <outToChannel>
                  <sFSwitchChannelMoniker name="/taskalarmworker/taskalarmworkerGroup/SW:FwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp" />
                </outToChannel>
              </outPort>
            </componentports>
            <settings>
              <aCS name="CloudToolsDiagnosticAgentVersion" defaultValue="" />
              <aCS name="ConnectionString" defaultValue="" />
              <aCS name="IntelliTrace.IntelliTraceConnectionString" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteDebugger.ClientThumbprint" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteDebugger.Connector.Enabled" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteDebugger.Connector.Version" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteDebugger.ServerThumbprint" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled" defaultValue="" />
              <aCS name="PushKey" defaultValue="" />
              <aCS name="PushSecret" defaultValue="" />
              <aCS name="StorageConnectionString" defaultValue="" />
              <aCS name="__ModelData" defaultValue="&lt;m role=&quot;FwWorkerRole&quot; xmlns=&quot;urn:azure:m:v1&quot;&gt;&lt;r name=&quot;FwWorkerRole&quot;&gt;&lt;e name=&quot;Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp&quot; /&gt;&lt;e name=&quot;Microsoft.WindowsAzure.Plugins.RemoteDebugger.Connector&quot; /&gt;&lt;e name=&quot;Microsoft.WindowsAzure.Plugins.RemoteDebugger.FileUpload&quot; /&gt;&lt;e name=&quot;Microsoft.WindowsAzure.Plugins.RemoteDebugger.Forwarder&quot; /&gt;&lt;e name=&quot;Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput&quot; /&gt;&lt;/r&gt;&lt;/m&gt;" />
            </settings>
            <resourcereferences>
              <resourceReference name="DiagnosticStore" defaultAmount="[4096,4096,4096]" defaultSticky="true" kind="Directory" />
              <resourceReference name="EventStore" defaultAmount="[1000,1000,1000]" defaultSticky="false" kind="LogStore" />
            </resourcereferences>
            <storedcertificates>
              <storedCertificate name="Stored0Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" certificateStore="My" certificateLocation="System">
                <certificate>
                  <certificateMoniker name="/taskalarmworker/taskalarmworkerGroup/FwWorkerRole/Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" />
                </certificate>
              </storedCertificate>
              <storedCertificate name="Stored1Microsoft.WindowsAzure.Plugins.RemoteDebugger.TransportValidation" certificateStore="My" certificateLocation="System">
                <certificate>
                  <certificateMoniker name="/taskalarmworker/taskalarmworkerGroup/FwWorkerRole/Microsoft.WindowsAzure.Plugins.RemoteDebugger.TransportValidation" />
                </certificate>
              </storedCertificate>
            </storedcertificates>
            <certificates>
              <certificate name="Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" />
              <certificate name="Microsoft.WindowsAzure.Plugins.RemoteDebugger.TransportValidation" />
            </certificates>
          </role>
          <sCSPolicy>
            <sCSPolicyIDMoniker name="/taskalarmworker/taskalarmworkerGroup/FwWorkerRoleInstances" />
            <sCSPolicyUpdateDomainMoniker name="/taskalarmworker/taskalarmworkerGroup/FwWorkerRoleUpgradeDomains" />
            <sCSPolicyFaultDomainMoniker name="/taskalarmworker/taskalarmworkerGroup/FwWorkerRoleFaultDomains" />
          </sCSPolicy>
        </groupHascomponents>
      </components>
      <sCSPolicy>
        <sCSPolicyUpdateDomain name="FwWorkerRoleUpgradeDomains" defaultPolicy="[5,5,5]" />
        <sCSPolicyFaultDomain name="FwWorkerRoleFaultDomains" defaultPolicy="[2,2,2]" />
        <sCSPolicyID name="FwWorkerRoleInstances" defaultPolicy="[1,1,1]" />
      </sCSPolicy>
    </group>
  </groups>
  <implements>
    <implementation Id="7b7e3083-213a-4eef-ba4a-d7bed1641a9a" ref="Microsoft.RedDog.Contract\ServiceContract\taskalarmworkerContract@ServiceDefinition">
      <interfacereferences>
        <interfaceReference Id="927c2c85-af4a-4b23-9b29-3cba646b578d" ref="Microsoft.RedDog.Contract\Interface\FwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput@ServiceDefinition">
          <inPort>
            <inPortMoniker name="/taskalarmworker/taskalarmworkerGroup/FwWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput" />
          </inPort>
        </interfaceReference>
      </interfacereferences>
    </implementation>
  </implements>
</serviceModel>