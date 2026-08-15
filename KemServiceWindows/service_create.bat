@echo off
echo Create the service
sc.exe create "KemService Windows" binpath= "%~dp0KemServiceWindows.exe" start= delayed-auto
pause