SET NETFX_PATH=C:\Windows\Microsoft.NET\Framework\v3.5

%NETFX_PATH%\MSBuild.exe "%~dp0..\src\reyna.sln" /property:Platform="Any CPU" /property:Configuration=Release
