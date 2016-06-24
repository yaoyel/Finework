<?xml version="1.0" encoding="utf-8"?>
<serviceModel xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" name="TaskAlarmService" generation="1" functional="0" release="0" Id="24d1b7b5-1e0d-4ae7-a17e-441186b91a23" dslVersion="1.2.0.0" xmlns="http://schemas.microsoft.com/dsltools/RDSM">
  <groups>
    <group name="TaskAlarmServiceGroup" generation="1" functional="0" release="0">
      <componentports>
        <inPort name="TaskAlarmWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput" protocol="tcp">
          <inToChannel>
            <lBChannelMoniker name="/TaskAlarmService/TaskAlarmServiceGroup/LB:TaskAlarmWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput" />
          </inToChannel>
        </inPort>
      </componentports>
      <settings>
        <aCS name="Certificate|TaskAlarmWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" defaultValue="">
          <maps>
            <mapMoniker name="/TaskAlarmService/TaskAlarmServiceGroup/MapCertificate|TaskAlarmWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" />
          </maps>
        </aCS>
        <aCS name="TaskAlarmWorkerRole:ConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/TaskAlarmService/TaskAlarmServiceGroup/MapTaskAlarmWorkerRole:ConnectionString" />
          </maps>
        </aCS>
        <aCS name="TaskAlarmWorkerRole:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/TaskAlarmService/TaskAlarmServiceGroup/MapTaskAlarmWorkerRole:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </maps>
        </aCS>
        <aCS name="TaskAlarmWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword" defaultValue="">
          <maps>
            <mapMoniker name="/TaskAlarmService/TaskAlarmServiceGroup/MapTaskAlarmWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword" />
          </maps>
        </aCS>
        <aCS name="TaskAlarmWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration" defaultValue="">
          <maps>
            <mapMoniker name="/TaskAlarmService/TaskAlarmServiceGroup/MapTaskAlarmWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration" />
          </maps>
        </aCS>
        <aCS name="TaskAlarmWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername" defaultValue="">
          <maps>
            <mapMoniker name="/TaskAlarmService/TaskAlarmServiceGroup/MapTaskAlarmWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername" />
          </maps>
        </aCS>
        <aCS name="TaskAlarmWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" defaultValue="">
          <maps>
            <mapMoniker name="/TaskAlarmService/TaskAlarmServiceGroup/MapTaskAlarmWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" />
          </maps>
        </aCS>
        <aCS name="TaskAlarmWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled" defaultValue="">
          <maps>
            <mapMoniker name="/TaskAlarmService/TaskAlarmServiceGroup/MapTaskAlarmWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled" />
          </maps>
        </aCS>
        <aCS name="TaskAlarmWorkerRole:PushKey" defaultValue="">
          <maps>
            <mapMoniker name="/TaskAlarmService/TaskAlarmServiceGroup/MapTaskAlarmWorkerRole:PushKey" />
          </maps>
        </aCS>
        <aCS name="TaskAlarmWorkerRole:PushSecret" defaultValue="">
          <maps>
            <mapMoniker name="/TaskAlarmService/TaskAlarmServiceGroup/MapTaskAlarmWorkerRole:PushSecret" />
          </maps>
        </aCS>
        <aCS name="TaskAlarmWorkerRole:StorageConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/TaskAlarmService/TaskAlarmServiceGroup/MapTaskAlarmWorkerRole:StorageConnectionString" />
          </maps>
        </aCS>
        <aCS name="TaskAlarmWorkerRoleInstances" defaultValue="[1,1,1]">
          <maps>
            <mapMoniker name="/TaskAlarmService/TaskAlarmServiceGroup/MapTaskAlarmWorkerRoleInstances" />
          </maps>
        </aCS>
      </settings>
      <channels>
        <lBChannel name="LB:TaskAlarmWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput">
          <toPorts>
            <inPortMoniker name="/TaskAlarmService/TaskAlarmServiceGroup/TaskAlarmWorkerRole/Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput" />
          </toPorts>
        </lBChannel>
        <sFSwitchChannel name="SW:TaskAlarmWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp">
          <toPorts>
            <inPortMoniker name="/TaskAlarmService/TaskAlarmServiceGroup/TaskAlarmWorkerRole/Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp" />
          </toPorts>
        </sFSwitchChannel>
      </channels>
      <maps>
        <map name="MapCertificate|TaskAlarmWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" kind="Identity">
          <certificate>
            <certificateMoniker name="/TaskAlarmService/TaskAlarmServiceGroup/TaskAlarmWorkerRole/Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" />
          </certificate>
        </map>
        <map name="MapTaskAlarmWorkerRole:ConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/TaskAlarmService/TaskAlarmServiceGroup/TaskAlarmWorkerRole/ConnectionString" />
          </setting>
        </map>
        <map name="MapTaskAlarmWorkerRole:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/TaskAlarmService/TaskAlarmServiceGroup/TaskAlarmWorkerRole/Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </setting>
        </map>
        <map name="MapTaskAlarmWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword" kind="Identity">
          <setting>
            <aCSMoniker name="/TaskAlarmService/TaskAlarmServiceGroup/TaskAlarmWorkerRole/Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword" />
          </setting>
        </map>
        <map name="MapTaskAlarmWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration" kind="Identity">
          <setting>
            <aCSMoniker name="/TaskAlarmService/TaskAlarmServiceGroup/TaskAlarmWorkerRole/Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration" />
          </setting>
        </map>
        <map name="MapTaskAlarmWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername" kind="Identity">
          <setting>
            <aCSMoniker name="/TaskAlarmService/TaskAlarmServiceGroup/TaskAlarmWorkerRole/Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername" />
          </setting>
        </map>
        <map name="MapTaskAlarmWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" kind="Identity">
          <setting>
            <aCSMoniker name="/TaskAlarmService/TaskAlarmServiceGroup/TaskAlarmWorkerRole/Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" />
          </setting>
        </map>
        <map name="MapTaskAlarmWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled" kind="Identity">
          <setting>
            <aCSMoniker name="/TaskAlarmService/TaskAlarmServiceGroup/TaskAlarmWorkerRole/Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled" />
          </setting>
        </map>
        <map name="MapTaskAlarmWorkerRole:PushKey" kind="Identity">
          <setting>
            <aCSMoniker name="/TaskAlarmService/TaskAlarmServiceGroup/TaskAlarmWorkerRole/PushKey" />
          </setting>
        </map>
        <map name="MapTaskAlarmWorkerRole:PushSecret" kind="Identity">
          <setting>
            <aCSMoniker name="/TaskAlarmService/TaskAlarmServiceGroup/TaskAlarmWorkerRole/PushSecret" />
          </setting>
        </map>
        <map name="MapTaskAlarmWorkerRole:StorageConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/TaskAlarmService/TaskAlarmServiceGroup/TaskAlarmWorkerRole/StorageConnectionString" />
          </setting>
        </map>
        <map name="MapTaskAlarmWorkerRoleInstances" kind="Identity">
          <setting>
            <sCSPolicyIDMoniker name="/TaskAlarmService/TaskAlarmServiceGroup/TaskAlarmWorkerRoleInstances" />
          </setting>
        </map>
      </maps>
      <components>
        <groupHascomponents>
          <role name="TaskAlarmWorkerRole" generation="1" functional="0" release="0" software="E:\FineWork\devel\sources\dotnet\main\FineWork.CloudService\TaskAlarmService\csx\Release\roles\TaskAlarmWorkerRole" entryPoint="base\x64\WaHostBootstrapper.exe" parameters="base\x64\WaWorkerHost.exe " memIndex="-1" hostingEnvironment="consoleroleadmin" hostingEnvironmentVersion="2">
            <componentports>
              <inPort name="Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput" protocol="tcp" />
              <inPort name="Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp" protocol="tcp" portRanges="3389" />
              <outPort name="TaskAlarmWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp" protocol="tcp">
                <outToChannel>
                  <sFSwitchChannelMoniker name="/TaskAlarmService/TaskAlarmServiceGroup/SW:TaskAlarmWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp" />
                </outToChannel>
              </outPort>
            </componentports>
            <settings>
              <aCS name="ConnectionString" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled" defaultValue="" />
              <aCS name="PushKey" defaultValue="" />
              <aCS name="PushSecret" defaultValue="" />
              <aCS name="StorageConnectionString" defaultValue="" />
              <aCS name="__ModelData" defaultValue="&lt;m role=&quot;TaskAlarmWorkerRole&quot; xmlns=&quot;urn:azure:m:v1&quot;&gt;&lt;r name=&quot;TaskAlarmWorkerRole&quot;&gt;&lt;e name=&quot;Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp&quot; /&gt;&lt;e name=&quot;Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput&quot; /&gt;&lt;/r&gt;&lt;/m&gt;" />
            </settings>
            <resourcereferences>
              <resourceReference name="DiagnosticStore" defaultAmount="[4096,4096,4096]" defaultSticky="true" kind="Directory" />
              <resourceReference name="EventStore" defaultAmount="[1000,1000,1000]" defaultSticky="false" kind="LogStore" />
            </resourcereferences>
            <storedcertificates>
              <storedCertificate name="Stored0Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" certificateStore="My" certificateLocation="System">
                <certificate>
                  <certificateMoniker name="/TaskAlarmService/TaskAlarmServiceGroup/TaskAlarmWorkerRole/Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" />
                </certificate>
              </storedCertificate>
            </storedcertificates>
            <certificates>
              <certificate name="Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" />
            </certificates>
          </role>
          <sCSPolicy>
            <sCSPolicyIDMoniker name="/TaskAlarmService/TaskAlarmServiceGroup/TaskAlarmWorkerRoleInstances" />
            <sCSPolicyUpdateDomainMoniker name="/TaskAlarmService/TaskAlarmServiceGroup/TaskAlarmWorkerRoleUpgradeDomains" />
            <sCSPolicyFaultDomainMoniker name="/TaskAlarmService/TaskAlarmServiceGroup/TaskAlarmWorkerRoleFaultDomains" />
          </sCSPolicy>
        </groupHascomponents>
      </components>
      <sCSPolicy>
        <sCSPolicyUpdateDomain name="TaskAlarmWorkerRoleUpgradeDomains" defaultPolicy="[5,5,5]" />
        <sCSPolicyFaultDomain name="TaskAlarmWorkerRoleFaultDomains" defaultPolicy="[2,2,2]" />
        <sCSPolicyID name="TaskAlarmWorkerRoleInstances" defaultPolicy="[1,1,1]" />
      </sCSPolicy>
    </group>
  </groups>
  <implements>
    <implementation Id="5e83ffe4-44bd-4295-818e-f567706ff5a9" ref="Microsoft.RedDog.Contract\ServiceContract\TaskAlarmServiceContract@ServiceDefinition">
      <interfacereferences>
        <interfaceReference Id="fcd965ed-61db-42f6-ab11-9e3bc17c63b4" ref="Microsoft.RedDog.Contract\Interface\TaskAlarmWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput@ServiceDefinition">
          <inPort>
            <inPortMoniker name="/TaskAlarmService/TaskAlarmServiceGroup/TaskAlarmWorkerRole:Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput" />
          </inPort>
        </interfaceReference>
      </interfacereferences>
    </implementation>
  </implements>
</serviceModel>