@ECHO OFF

set VERSION=%~1
set BUILD_FOLDER_NAME=v%VERSION%

set BACKEND_NAME=backend
set BACKEND_SRC=src/%BACKEND%
set BACKEND_OUT=%BUILD_FOLDER_NAME%/%BACKEND_NAME%
set BACKEND_OUT_FROM_PROJ=../../%BACKEND_OUT%

set FRONTEND_NAME=frontend
set FRONTEND_SRC=src/%FRONTEND_NAME%
set FRONTEND_OUT=%BUILD_FOLDER_NAME%/%FRONTEND_NAME%
set FRONTEND_OUT_FROM_PROJ=../../%FRONTEND_OUT%

CALL :MakeOutDir
CALL :Build                                                       
CALL :CreateRunScript
CALL :CreateStopScript

:MakeOutDir
mkdir %BUILD_FOLDER_NAME%
EXIT /B 0

:Build
dotnet build %BACKEND_SRC% -o %BACKEND_OUT_FROM_PROJ%
dotnet build %FRONTEND_SRC% -o %FRONTEND_OUT_FROM_PROJ%
EXIT /B 0

:CreateRunScript
(
@echo start dotnet %FRONTEND_NAME%/%FRONTEND_NAME%.dll
@echo start dotnet %BACKEND_NAME%/%BACKEND_NAME%.dll
) > %BUILD_FOLDER_NAME%/run.cmd
EXIT /B 0

:CreateStopScript
@echo dotnet > %BUILD_FOLDER_NAME%/stop.cmd
EXIT /B 0

                              