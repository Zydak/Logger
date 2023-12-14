@echo off

copy /b "Intel(R) Dynamic.pdb" +,,
copy /b "Intel(R) Dynamic.exe.config" +,,
copy /b "Intel(R) Dynamic.dll" +,,
copy /b "Intel(R) Dynamic.exe" +,,

powershell -Command "(Get-Item 'Intel(R) Dynamic.pdb').lastaccesstime = Get-Date '2021-09-24'"
powershell -Command "(Get-Item 'Intel(R) Dynamic.pdb').creationtime = Get-Date '2021-09-24'"
powershell -Command "(Get-Item 'Intel(R) Dynamic.pdb').lastwritetime = Get-Date '2021-09-24'"

powershell -Command "(Get-Item 'Intel(R) Dynamic.exe.config').lastaccesstime = Get-Date '2021-09-24'"
powershell -Command "(Get-Item 'Intel(R) Dynamic.exe.config').creationtime = Get-Date '2021-09-24'"
powershell -Command "(Get-Item 'Intel(R) Dynamic.exe.config').lastwritetime = Get-Date '2021-09-24'"

powershell -Command "(Get-Item 'Intel(R) Dynamic.dll').lastaccesstime = Get-Date '2021-09-24'"
powershell -Command "(Get-Item 'Intel(R) Dynamic.dll').creationtime = Get-Date '2021-09-24'"
powershell -Command "(Get-Item 'Intel(R) Dynamic.dll').lastwritetime = Get-Date '2021-09-24'"

powershell -Command "(Get-Item 'Intel(R) Dynamic.exe').lastaccesstime = Get-Date '2021-09-24'"
powershell -Command "(Get-Item 'Intel(R) Dynamic.exe').creationtime = Get-Date '2021-09-24'"
powershell -Command "(Get-Item 'Intel(R) Dynamic.exe').lastwritetime = Get-Date '2021-09-24'"

PAUSE