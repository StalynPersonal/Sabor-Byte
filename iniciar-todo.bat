@echo off
REM Levanta la Api y las 4 apps (Central, Caja, Cocina, Mesero), cada una en su propia
REM ventana de consola. Cerrar la ventana correspondiente para detener esa app.
REM
REM Compila TODA la solucion una sola vez antes de arrancar nada: Central/Caja/Cocina/
REM Mesero comparten el proyecto SaborByte.Web.Api (y SaborByte.Web.UI) — si se lanzan
REM los 5 "dotnet run" al mismo tiempo, cada uno intenta compilar esas librerias
REM compartidas por su cuenta y se pisan el .dll en obj\ (error CS2012, archivo
REM bloqueado por VBCSCompiler). Compilando antes y arrancando con --no-build se evita
REM la carrera por completo.

set RAIZ=%~dp0

echo Compilando la solucion completa (una sola vez, para evitar choques entre las apps)...
dotnet build "%RAIZ%SaborByte.slnx"
if errorlevel 1 (
    echo.
    echo La compilacion fallo. Revisa los errores de arriba antes de continuar.
    pause
    exit /b 1
)

start "SaborByte.Api (5080)" cmd /k "cd /d "%RAIZ%Api" && dotnet run --no-build --urls http://localhost:5080"
start "SaborByte.Web.Central (5090)" cmd /k "cd /d "%RAIZ%Web\Apps\SaborByte.Web.Central" && dotnet run --no-build --urls http://localhost:5090"
start "SaborByte.Web.Caja (5091)" cmd /k "cd /d "%RAIZ%Web\Apps\SaborByte.Web.Caja" && dotnet run --no-build --urls http://localhost:5091"
start "SaborByte.Web.Cocina (5092)" cmd /k "cd /d "%RAIZ%Web\Apps\SaborByte.Web.Cocina" && dotnet run --no-build --urls http://localhost:5092"
start "SaborByte.Web.Mesero (5093)" cmd /k "cd /d "%RAIZ%Web\Apps\SaborByte.Web.Mesero" && dotnet run --no-build --urls http://localhost:5093"

echo.
echo Se abrieron 5 ventanas: Api (5080), Central (5090), Caja (5091), Cocina (5092), Mesero (5093).
echo Cierra cada ventana para detener esa app.
