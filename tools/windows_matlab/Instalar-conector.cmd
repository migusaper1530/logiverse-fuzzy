@echo off
setlocal
set "CONNECTOR_DIR=%LOCALAPPDATA%\LogiVerseMatlab"
if not exist "%~dp0LogiVerseMatlab.exe" (
  echo Extrae todos los archivos del ZIP antes de ejecutar este instalador.
  pause
  exit /b 1
)
if not exist "%CONNECTOR_DIR%" mkdir "%CONNECTOR_DIR%"
copy /Y "%~dp0LogiVerseMatlab.exe" "%CONNECTOR_DIR%\LogiVerseMatlab.exe" >nul
if errorlevel 1 (
  echo No se pudo copiar el conector. Cierra sus ventanas y vuelve a intentarlo.
  pause
  exit /b 1
)
start "" /wait "%CONNECTOR_DIR%\LogiVerseMatlab.exe" --install
