@ECHO OFF
dotnet publish -c release -o ..\Web\
REM dotnet publish -c release -r win81-x86 -o ..\Web\
REM Without -r parameter: Framework-dependent deployment (FDD), else Self-contained deployment (SCD)
REM dotnet publish -c release -v detailed -r win81-x86 -o ..\Web\
pause

