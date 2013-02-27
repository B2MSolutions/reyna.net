@SET CONFIGURATION=Release
@SET NETFX_PATH=C:\Windows\Microsoft.NET\Framework\v3.5
@SET XUNIT_VERSION=1.9.1

@ECHO Compiling solution
%NETFX_PATH%\MSBuild.exe "%~dp0..\src\reyna.sln" /property:Platform="Any CPU" /property:Configuration=%CONFIGURATION%
@SET EXITCODE=%ERRORLEVEL%
@IF %ERRORLEVEL% NEQ 0 GOTO error

@ECHO Running tests
"%~dp0..\tools\xUnit\%XUNIT_VERSION%\xunit.console.exe" "%~dp0..\src\reyna.Facts\bin\%CONFIGURATION%\reyna.Facts.dll"
@SET EXITCODE=%ERRORLEVEL%
@IF %ERRORLEVEL% NEQ 0 GOTO error

@GOTO exit

:error
@ECHO build script return code: %EXITCODE%
@EXIT /B %EXITCODE%

:exit
@ECHO build succeeded
