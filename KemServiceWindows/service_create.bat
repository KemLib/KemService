@echo off
echo Create the service
sc.exe create "KemService Windows" binpath= "F:\Project\KemLib\KemService\KemService\KemServiceWindows\bin\Release\net10.0\KemServiceWindows.exe"
pause