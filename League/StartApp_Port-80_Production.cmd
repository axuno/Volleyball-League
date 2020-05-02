cd ..\Web\
set ASPNETCORE_ENVIRONMENT=Production
set ASPNETCORE_URLS=http://localhost:80;http://axuno1:80
dotnet League.dll
pause

