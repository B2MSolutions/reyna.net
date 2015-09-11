@SET CONFIGURATION=Release
@SET NETFX_PATH=C:\Windows\Microsoft.NET\Framework\v3.5
@SET XUNIT_VERSION=1.9.1
@SET OPENCOVER_VERSION=4.5.1604
@SET REPORTGENERATOR_VERSION=1.8.1.0

@ECHO Compiling solution
%NETFX_PATH%\MSBuild.exe "%~dp0..\src\reyna.sln" /property:Platform="Any CPU" /property:Configuration=%CONFIGURATION%
@SET EXITCODE=%ERRORLEVEL%
@IF %ERRORLEVEL% NEQ 0 GOTO error

@SET COVERAGE_DIRECTORY=%~dp0..\Coverage
rmdir /S /Q %COVERAGE_DIRECTORY%
mkdir %COVERAGE_DIRECTORY%

@ECHO Running unit tests
"%~dp0..\tools\opencover\%OPENCOVER_VERSION%\opencover.console.exe" -target:"%~dp0..\tools\xUnit\%XUNIT_VERSION%\xunit.console.x86.exe" -targetargs:"%~dp0..\src\reyna.Facts\bin\%CONFIGURATION%\reyna.Facts.dll /xml ""%COVERAGE_DIRECTORY%/unit-output.xml"" /html ""%COVERAGE_DIRECTORY%/unit-output.html"" /nunit ""%COVERAGE_DIRECTORY%/unit-output.nunit.xml"" /noshadow" -filter:"+[Reyna*]* -[*.Facts]*" -showunvisited -output:"%COVERAGE_DIRECTORY%\coverage-unit.xml" -returntargetcode
@SET EXITCODE=%ERRORLEVEL%
@IF %ERRORLEVEL% NEQ 0 GOTO error

@ECHO Running integration tests
"%~dp0..\tools\opencover\%OPENCOVER_VERSION%\opencover.console.exe" -target:"%~dp0..\tools\xUnit\%XUNIT_VERSION%\xunit.console.x86.exe" -targetargs:"%~dp0..\src\reyna.Integration.Facts\bin\%CONFIGURATION%\reyna.Integration.Facts.dll /xml ""%COVERAGE_DIRECTORY%/integration-output.xml"" /html ""%COVERAGE_DIRECTORY%/integration-output.html"" /nunit ""%COVERAGE_DIRECTORY%/integration-output.nunit.xml"" /noshadow" -filter:"+[Reyna*]* -[*.Facts]* -[Reyna.Win32]Reyna.EncryptionChecker -[Reyna.Win32]Reyna.ReynaService -[Reyna.Win32]Reyna.SQLiteRepository" -showunvisited -output:"%COVERAGE_DIRECTORY%\coverage-integration.xml" -returntargetcode
@SET EXITCODE=%ERRORLEVEL%
@IF %ERRORLEVEL% NEQ 0 GOTO error

@ECHO Generating coverage report
"%~dp0..\tools\ReportGenerator\%REPORTGENERATOR_VERSION%\ReportGenerator.exe" -reports:"%COVERAGE_DIRECTORY%\coverage-unit.xml";"%COVERAGE_DIRECTORY%\coverage-integration.xml" coveragereport -reporttypes:Html;xmlsummary;HtmlSummary -targetdir:"%COVERAGE_DIRECTORY%" -verbosity:Verbose
@SET EXITCODE=%ERRORLEVEL%
@IF %ERRORLEVEL% NEQ 0 GOTO error

@ECHO Coverage report
type "%COVERAGE_DIRECTORY%\summary.xml"

findstr.exe /L "<Coverage>100%%</Coverage>" "%COVERAGE_DIRECTORY%\summary.xml"
@SET FindStrResult=%ERRORLEVEL%
@IF %FindStrResult% EQU 0 (
  @GOTO exit
) ELSE (
  @GOTO coverageNotMet
)

:error
@ECHO build script return code: %EXITCODE%
@EXIT /B %EXITCODE%

:exit
@ECHO Coverage requirements met      
@ECHO build succeeded
@EXIT /B 0

:coverageNotMet
@ECHO Coverage requirements not met, to view coverage result click on %COVERAGE_DIRECTORY%\index.htm
@EXIT /B %EXITCODE%