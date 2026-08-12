@echo off
echo Configure the service
sc.exe failure "KemService Windows" reset= 0 actions= restart/60000/restart/60000/run/1000
pause