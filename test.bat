@echo off
powershell -Command "$folderPath = 'C:\Dev\logger'; Get-ChildItem -Path $folderPath | ForEach-Object { Write-Host 'File:', $_.FullName; Write-Host '  LastAccessTime:', $_.LastAccessTime; Write-Host '  CreationTime:', $_.CreationTime; Write-Host '  LastWriteTime:', $_.LastWriteTime; Write-Host '' }"
PAUSE