@echo off
cd /d "%~dp0"
dotnet build -c Debug -p:Platform=x64 || exit /b 1
echo.
echo If the window does not open or you see Class not registered, install Windows App Runtime 1.8 x64:
echo https://learn.microsoft.com/windows/apps/windows-app-sdk/downloads
echo.
echo Easiest path: open TeamNut in Visual Studio, pick profile TeamNut (Unpackaged), press F5.
echo.
cd bin\x64\Debug\net8.0-windows10.0.19041.0\win-x64
dotnet exec TeamNut.dll
