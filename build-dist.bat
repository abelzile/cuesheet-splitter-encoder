@echo off

msbuild src\cuesheet-splitter-encoder-vs2013.sln /t:Rebuild /p:Configuration=Release /p:Platform="Any CPU"

del dist\*.* /F /Q

xcopy  src\CuesheetSplitterEncoder.CmdUi\bin\Release\*.* dist