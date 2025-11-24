@echo off
echo ==============================
echo   CLEANING PROJECT
echo ==============================
dotnet clean
echo ==============================
echo   BUILDING PROJECT
echo ==============================
dotnet build
echo ==============================
echo   RUNNING PROJECT
echo ==============================
dotnet run
