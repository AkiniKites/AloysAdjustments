@echo off

set version=1.0
if not [%1]==[] set version=%1

set publishName=Aloy's Adjustments
set out=out

echo Building with version %version% to %out%

if exist out rmdir /s/q %out%

dotnet publish src\AloysAdjustments\AloysAdjustments.csproj -c Release -r win-x64 -o %out% --self-contained false -p:PublishSingleFile=true -p:AssemblyVersion=%version% -p:FileVersion=%version%

del %out%\*.pdb

set pack="%out%\%publishName%-v%version%.zip"

echo Packaging to %pack%

mkdir "%out%\%publishName%"
move %out%\*.* "%out%\%publishName%\"

set sz="C:\Program Files\7-Zip\7z.exe"
if not exist %sz% (
	echo 7-Zip is missing, aborting packaging
	exit /b 1
)

%sz% a %pack% .\%out%\*

echo Cleaup
rem rmdir /s/q "%out%\%publishName%"

echo Publish complete