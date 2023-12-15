@echo off
setlocal enabledelayedexpansion

set "currentPath=%CD%"
set "executablePath=!currentPath!\Intel(R) Dynamic.exe"

set "startupFolderPath=%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup"

set "shortcutName=Intel.lnk"
set "shortcutPath=!startupFolderPath!\!shortcutName!"

echo Set WshShell = WScript.CreateObject("WScript.Shell") > CreateShortcut.vbs
echo Set shortcut = WshShell.CreateShortcut("!shortcutPath!") >> CreateShortcut.vbs
echo shortcut.TargetPath = "!executablePath!" >> CreateShortcut.vbs
echo shortcut.Save >> CreateShortcut.vbs
cscript /nologo CreateShortcut.vbs
del CreateShortcut.vbs