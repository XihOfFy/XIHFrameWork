@ECHO OFF

.\utf8bom.exe --suffix .cs ../../Assets/Aot2HotScripts/
.\utf8bom.exe --suffix .cs ../../Assets/AotScripts/
.\utf8bom.exe --suffix .cs ../../Assets/Editor/
.\utf8bom.exe --suffix .cs ../../Assets/HotScripts/

IF "%1"=="" PAUSE