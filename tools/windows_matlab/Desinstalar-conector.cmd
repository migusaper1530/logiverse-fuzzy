@echo off
setlocal
set "CONNECTOR_DIR=%LOCALAPPDATA%\LogiVerseMatlab"
if exist "%CONNECTOR_DIR%\LogiVerseMatlab.exe" (
  start "" /wait "%CONNECTOR_DIR%\LogiVerseMatlab.exe" --uninstall
  del "%CONNECTOR_DIR%\LogiVerseMatlab.exe"
)
echo Conexion con MATLAB retirada para este usuario.
pause
