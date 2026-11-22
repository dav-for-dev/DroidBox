@echo off
title DroidBox - Dev Mode
cd /d "%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\run-dev.ps1"
pause
