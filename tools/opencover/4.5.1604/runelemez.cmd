opencover.console -target:"C:\elemez-ce\tools\ThirdParty\xunit\1.9.1\xunit.console.x86.exe" -targetargs:"C:\elemez-ce\src\ElemezTest\bin\Release\elemeztest.dll" -filter:"+[Elemez*]* -[ElemezTest*]*" -showunvisited -output:coverage.xml -returntargetcode
echo process returned %ERRORLEVEL% 

REM -output:coverage.xml 