@echo off
REM Script per gestire il setup del progetto con Docker su Windows

setlocal enabledelayedexpansion

echo.
echo ?? StudySync Docker Setup
echo =========================
echo.

REM Verifica se Docker è installato
docker --version >nul 2>&1
if errorlevel 1 (
    echo ? Docker non è installato. Installalo da https://www.docker.com/
    pause
    exit /b 1
)

REM Verifica se Docker Compose è installato
docker-compose --version >nul 2>&1
if errorlevel 1 (
    echo ? Docker Compose non è installato.
    pause
    exit /b 1
)

echo ? Docker e Docker Compose trovati
echo.
echo Seleziona un'operazione:
echo 1) Avvia i container (build + up)
echo 2) Ferma i container
echo 3) Ricostruisci i container
echo 4) Applica le migrazioni al database
echo 5) Visualizza i log
echo 6) Pulisci tutto (remove containers, volumes, networks)
echo 7) Esci
echo.
set /p choice="Inserisci il numero dell'operazione (1-7): "

if "%choice%"=="1" (
    echo.
    echo ?? Avvio dei container...
    docker-compose up -d --build
    echo.
    echo ? Container avviati con successo!
    echo.
    echo URL dell'applicazione:
    echo   - HTTP:  http://localhost:5000
    echo   - HTTPS: https://localhost:5001
    echo.
    echo Credenziali MySQL:
    echo   - Host: mysql (da dentro Docker) / localhost:3306 (da host)
    echo   - User: root
    echo   - Password: password_super_segreta
    echo   - Database: studysync_db
    echo.
    pause
) else if "%choice%"=="2" (
    echo.
    echo ??  Fermo i container...
    docker-compose down
    echo ? Container fermati
    echo.
    pause
) else if "%choice%"=="3" (
    echo.
    echo ?? Ricostruisco i container...
    docker-compose up -d --build --force-recreate
    echo ? Container ricostruiti
    echo.
    pause
) else if "%choice%"=="4" (
    echo.
    echo ???  Applico le migrazioni al database...
    docker-compose exec -T app dotnet ef database update -p Template -s Template.Web
    echo ? Migrazioni applicate
    echo.
    pause
) else if "%choice%"=="5" (
    echo.
    echo ?? Visualizzo i log dell'app...
    docker-compose logs -f app
) else if "%choice%"=="6" (
    echo.
    echo ??  ATTENZIONE: Questa operazione eliminerà tutto!
    set /p confirm="Sei sicuro? (s/n): "
    if /i "%confirm%"=="s" (
        echo.
        echo ???  Pulisco tutto...
        docker-compose down -v
        echo ? Tutto pulito
    ) else (
        echo Operazione annullata
    )
    echo.
    pause
) else if "%choice%"=="7" (
    echo Arrivederci!
    exit /b 0
) else (
    echo ? Opzione non valida
    pause
    exit /b 1
)

goto :eof
