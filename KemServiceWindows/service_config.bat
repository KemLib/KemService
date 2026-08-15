@echo off
echo Configure the service
sc description "KemService Windows" "My Description"
sc.exe failure "KemService Windows" reset= 0 actions= restart/60000/restart/60000
pause