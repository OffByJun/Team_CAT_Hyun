@echo off
setlocal
cd /d "%~dp0"
git pull --ff-only --recurse-submodules
if errorlevel 1 goto fail
git submodule update --init --recursive
if errorlevel 1 goto fail
echo Project and SchoolSync updated.
pause
exit /b 0
:fail
echo Update stopped. Check local changes, branch divergence, or network access.
pause
exit /b 1
