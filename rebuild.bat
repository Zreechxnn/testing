@echo off
echo ==============================
echo   CLEANING PROJECT
echo ==============================
dotnet clean

if %ERRORLEVEL% NEQ 0 (
    echo ❌ Clean failed
    goto :error
)

echo ==============================
echo   DROPPING DATABASE
echo ==============================
dotnet ef database drop -f

if %ERRORLEVEL% NEQ 0 (
    echo ⚠️ Database drop failed or no database exists
)

echo ==============================
echo   DELETING OLD MIGRATIONS
echo ==============================
if exist Migrations (
    rmdir /s /q Migrations
    echo ✅ Migrations folder deleted.
) else (
    echo ℹ️ No Migrations folder found.
)

echo ==============================
echo   BUILDING PROJECT
echo ==============================
dotnet build

if %ERRORLEVEL% NEQ 0 (
    echo ❌ Build failed
    goto :error
)

echo ==============================
echo   CREATING NEW MIGRATION
echo ==============================
dotnet ef migrations add InitialCreate

if %ERRORLEVEL% NEQ 0 (
    echo ❌ Migration creation failed
    goto :error
)

echo ==============================
echo   UPDATING DATABASE
echo ==============================
dotnet ef database update

if %ERRORLEVEL% NEQ 0 (
    echo ❌ Database update failed
    goto :error
)

echo ==============================
echo   RUNNING PROJECT
echo ==============================
echo ✅ Rebuild completed successfully!
echo Starting application...
dotnet run

if %ERRORLEVEL% NEQ 0 (
    echo ❌ Application failed to start
    goto :error
)

goto :end

:error
echo.
echo ==============================
echo   REBUILD FAILED
echo ==============================
echo Please fix the errors and try again.
pause
exit /b 1

:end
pause