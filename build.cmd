@ECHO OFF

IF "%~1"=="" (
  GOTO InvalidArgs
)

SET BUILD_DIR=v%~1
SET CONFIG_DIR=config
SET CONFIG_EXT=json
SET SRC_DIR=src

SET BACKEND_NAME=backend
SET BACKEND_CONFIG=%SRC_DIR%\%BACKEND_NAME%\config.%CONFIG_EXT%
SET BACKEND_WINDOW_NAME=%BACKEND_NAME% %BUILD_DIR%

SET FRONTEND_NAME=frontend
SET FRONTEND_CONFIG=%SRC_DIR%\%FRONTEND_NAME%\config.%CONFIG_EXT%
SET FRONTEND_WINDOW_NAME=%FRONTEND_NAME% %BUILD_DIR%

SET TEXT_LISTENER_NAME=textListener

SET TEXT_RANK_CALC_NAME=textRankCalc

CALL :Clear

CALL :Build
IF %ERRORLEVEL% NEQ 0 GOTO BuildError

CALL :CopyConfig
IF %ERRORLEVEL% NEQ 0 GOTO CopyConfigsError

CALL :CreateRunScript
CALL :CreateStopScript

ECHO Build completed!
EXIT /B 0

:Clear
  IF EXIST %BUILD_DIR% RD /s /q "%BUILD_DIR%"
  EXIT /B 0

:Build
  dotnet publish %SRC_DIR%\%BACKEND_NAME% -c Release -o ..\..\%BUILD_DIR%\%BACKEND_NAME%
  IF %ERRORLEVEL% NEQ 0 EXIT /B 1
  dotnet publish %SRC_DIR%\%FRONTEND_NAME% -c Release -o ..\..\%BUILD_DIR%\%FRONTEND_NAME%
  IF %ERRORLEVEL% NEQ 0 EXIT /B 1
  dotnet build %SRC_DIR%\%TEXT_LISTENER_NAME% -o ..\..\%BUILD_DIR%\%TEXT_LISTENER_NAME%
  IF %ERRORLEVEL% NEQ 0 EXIT /B 1
  dotnet build %SRC_DIR%\%TEXT_RANK_CALC_NAME% -o ..\..\%BUILD_DIR%\%TEXT_RANK_CALC_NAME%
  IF %ERRORLEVEL% NEQ 0 EXIT /B 1
  EXIT /B 0
  
:CopyConfig
  MD "%BUILD_DIR%\%CONFIG_DIR%"
  COPY "%BACKEND_CONFIG%" "%BUILD_DIR%\%CONFIG_DIR%\%BACKEND_NAME%.%CONFIG_EXT%"
  IF %ERRORLEVEL% NEQ 0 EXIT /B 1
  COPY "%FRONTEND_CONFIG%" "%BUILD_DIR%\%CONFIG_DIR%\%FRONTEND_NAME%.%CONFIG_EXT%"
  IF %ERRORLEVEL% NEQ 0 EXIT /B 1
  EXIT /B 0s

:CreateRunScript
  (
    @ECHO copy "%CONFIG_DIR%\%BACKEND_NAME%.%CONFIG_EXT%" "%BACKEND_NAME%\config.%CONFIG_EXT%"
    @ECHO copy "%CONFIG_DIR%\%FRONTEND_NAME%.%CONFIG_EXT%" "%FRONTEND_NAME%\config.%CONFIG_EXT%"
    @ECHO start "%FRONTEND_WINDOW_NAME%" dotnet %FRONTEND_NAME%\%FRONTEND_NAME%.dll
    @ECHO start "%BACKEND_WINDOW_NAME%" dotnet %BACKEND_NAME%\%BACKEND_NAME%.dll
    @ECHO start "%TEXT_LISTENER_NAME%" dotnet %TEXT_LISTENER_NAME%\%TEXT_LISTENER_NAME%.dll
    @ECHO start "%TEXT_RANK_CALC_NAME%" dotnet %TEXT_RANK_CALC_NAME%\%TEXT_RANK_CALC_NAME%.dll
  ) > %BUILD_DIR%\run.cmd
  EXIT /B 0

:CreateStopScript
  (
    @ECHO @ECHO OFF
    @ECHO taskkill /IM dotnet.exe
  ) > %BUILD_DIR%\stop.cmd
  EXIT /B 0

:InvalidArgs
  ECHO Invalid build version. Usage: build.cmd version
  EXIT /B 1

:BuildError
  ECHO Error during build project...
  CALL :Clear
  EXIT /B 2

:CopyConfigsError
  ECHO Error during copy config files...
  CALL :Clear
  EXIT /B 3
