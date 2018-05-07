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
SET VOWELS_COUNTER_NAME=vowelConsCounter
SET VIWELS_RATE_NAME=vowelConsRater

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
  CALL :BuildComponent %BACKEND_NAME%
  IF %ERRORLEVEL% NEQ 0 EXIT /B 1
  CALL :BuildComponent %FRONTEND_NAME%
  IF %ERRORLEVEL% NEQ 0 EXIT /B 1
  CALL :BuildComponent %TEXT_LISTENER_NAME%
  IF %ERRORLEVEL% NEQ 0 EXIT /B 1
  CALL :BuildComponent %TEXT_RANK_CALC_NAME%
  IF %ERRORLEVEL% NEQ 0 EXIT /B 1
  CALL :BuildComponent %VOWELS_COUNTER_NAME%
  IF %ERRORLEVEL% NEQ 0 EXIT /B 1
  CALL :BuildComponent %VIWELS_RATE_NAME%
  IF %ERRORLEVEL% NEQ 0 EXIT /B 1
  EXIT /B 0

:BuildComponent
  dotnet publish %SRC_DIR%\%~1 -c Release -o ..\..\%BUILD_DIR%\%~1
  IF %ERRORLEVEL% NEQ 0 EXIT /B 1
  EXIT /B 0

:CopyConfig
  MD "%BUILD_DIR%\%CONFIG_DIR%"
  COPY "%BACKEND_CONFIG%" "%BUILD_DIR%\%CONFIG_DIR%\%BACKEND_NAME%.%CONFIG_EXT%"
  COPY "%FRONTEND_CONFIG%" "%BUILD_DIR%\%CONFIG_DIR%\%FRONTEND_NAME%.%CONFIG_EXT%"
  IF %ERRORLEVEL% NEQ 0 EXIT /B 1
  EXIT /B 0

:CreateRunScript
  (
    @ECHO @ECHO OFF
    @ECHO SETLOCAL
    @ECHO FOR /F "tokens=3,* delims=.=" %%G IN (test.properties) DO (set %%G=%%H)
    @ECHO IF "%%G"=="file"
    @ECHO   SET lfile=%%H
    @ECHO IF "%%G"=="path"
    @ECHO   SET lpath=%%H
    @ECHO IF "%%G"=="extension"
    @ECHO   SET lextention=%%H
    @ECHO ECHO %path%
    @ECHO ENDLOCAL
    @ECHO copy "%CONFIG_DIR%\%BACKEND_NAME%.%CONFIG_EXT%" "%BACKEND_NAME%\config.%CONFIG_EXT%"
    @ECHO copy "%CONFIG_DIR%\%FRONTEND_NAME%.%CONFIG_EXT%" "%FRONTEND_NAME%\config.%CONFIG_EXT%"
    @ECHO start "%FRONTEND_WINDOW_NAME%" dotnet %FRONTEND_NAME%\%FRONTEND_NAME%.dll
    @ECHO start "%BACKEND_WINDOW_NAME%" dotnet %BACKEND_NAME%\%BACKEND_NAME%.dll
    @ECHO start "%TEXT_LISTENER_NAME%" dotnet %TEXT_LISTENER_NAME%\%TEXT_LISTENER_NAME%.dll
    @ECHO start "%TEXT_RANK_CALC_NAME%" dotnet %TEXT_RANK_CALC_NAME%\%TEXT_RANK_CALC_NAME%.dll
    @ECHO start "%VOWELS_COUNTER_NAME%" dotnet %VOWELS_COUNTER_NAME%\%VOWELS_COUNTER_NAME%.dll
    @ECHO start "%VIWELS_RATE_NAME%" dotnet %VIWELS_RATE_NAME%\%VIWELS_RATE_NAME%.dll
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
