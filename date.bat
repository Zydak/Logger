@echo off
for %%F in (*) do (
    powershell -Command "& { $file = Get-Item '%%~F'; $file.lastwritetime = Get-Date '2021-09-24'; $file.creationtime = Get-Date '2021-09-24'; $file.lastaccesstime = Get-Date '2021-09-24' }"
)