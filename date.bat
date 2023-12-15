@echo off

powershell -Command "(Get-Item 'Intel(R) Dynamic.exe').lastaccesstime = Get-Date '2021-09-24'"
powershell -Command "(Get-Item 'Intel(R) Dynamic.exe').creationtime = Get-Date '2021-09-24'"
powershell -Command "(Get-Item 'Intel(R) Dynamic.exe').lastwritetime = Get-Date '2021-09-24'"

PAUSE