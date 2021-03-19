@ECHO OFF
dotnet publish  --no-self-contained -v minimal -c release -f netcoreapp3.1 -o ..\..\Web\ -p:Platform=x64 -p:EnvironmentName=Production
REM dotnet publish -c release -r win81-x86 -o ..\Web\
REM Without -r parameter: Framework-dependent deployment (FDD), else Self-contained deployment (SCD)
REM dotnet publish -c release -v detailed -r win81-x86 -o ..\Web\
pause

