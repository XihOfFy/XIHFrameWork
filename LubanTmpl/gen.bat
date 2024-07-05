set WORKSPACE=..
set LUBAN_DLL=%WORKSPACE%\LubanTmpl\Tools\Luban.dll
set CONF_ROOT=.
set OUT_DATA=..\Assets\Res\Tmpl
set OUT_CODE=..\Assets\HotScripts\Luban\Tmpl

dotnet %LUBAN_DLL% ^
    -t client ^
	-c cs-bin ^
    -d bin ^
    --conf %CONF_ROOT%\luban.conf ^
	-x outputCodeDir=%OUT_CODE% ^
    -x outputDataDir=%OUT_DATA%

pause