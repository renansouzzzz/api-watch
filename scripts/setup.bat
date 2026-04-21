@echo off
echo ============================================
echo  ApiWatch - Setup Inicial
echo ============================================

echo.
echo [1/3] Subindo container PostgreSQL...
cd ..\docker
docker-compose up -d postgres
timeout /t 5 /nobreak > NUL

echo.
echo [2/3] Aplicando migrations do banco...
cd ..\src\ApiWatch.Api
dotnet ef database update

echo.
echo [3/3] Tudo pronto!
echo.
echo  Postgres:  localhost:5432
echo  pgAdmin:   http://localhost:5050  (admin@apiwatch.local / admin123)
echo.
echo  Para rodar a API:
echo    cd src\ApiWatch.Api ^& dotnet run
echo.
echo  Para rodar o Worker (outro terminal):
echo    cd src\ApiWatch.Worker ^& dotnet run
echo.
pause
