@echo off
echo Criando migration: %1
cd ..\src\ApiWatch.Api
dotnet ef migrations add %1 --project ..\ApiWatch.Core\ApiWatch.Core.csproj --startup-project .
echo Migration criada com sucesso!
pause
