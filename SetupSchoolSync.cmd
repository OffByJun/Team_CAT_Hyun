@echo off
setlocal
cd /d "%~dp0"
git submodule update --init --recursive
if errorlevel 1 goto fail
git config --local submodule.recurse true
if errorlevel 1 goto fail
echo SchoolSync is ready. Future git pull will update initialized submodules.
pause
exit /b 0
:fail
echo Setup failed. Check Git installation and repository access.
pause
exit /b 1
