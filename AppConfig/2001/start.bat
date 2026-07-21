@title 打包Bat执行
@echo off

call ..\PACK_PRE_OPT\preopt.bat

@set SRC_DIR=%CD%\..\..\

@echo %SRC_DIR%


@echo 替换原先文件夹
xcopy /E /I /C /Y ..\SDK_TT %SRC_DIR%
xcopy /E /I /C /Y .\Proj %SRC_DIR%

@echo 处理完成!
pause
@echo on