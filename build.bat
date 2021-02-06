@echo off

set version=1.0
if not [%1]==[] set version=%1

set publishName=Aloy's Adjustments
set out=out
set dirp=plugins
set outp=%out%\%dirp%

echo Building with version %version% to %out%

if exist %out% rmdir /s/q %out%

dotnet publish src\AloysAdjustments\AloysAdjustments.csproj -c Release -r win-x64 -o %out% --self-contained false -p:PublishSingleFile=true -p:AssemblyVersion=%version% -p:FileVersion=%version%

set pluginout=%out%\tmp
	
for /d %%P in (src\Plugins\*) do ( 
	echo Building Plugin %%~nxP
	if exist %pluginout% rmdir /s/q %pluginout%
	
	dotnet build %%P\%%~nxP.csproj -c Release -r win-x64 -o "%pluginout%" -p:AssemblyVersion=%version% -p:FileVersion=%version%
	
	if not exist "%outp%" mkdir "%outp%"
	
	::copy all needed libraries
	copy "%pluginout%\%%~nxP.dll" "%outp%\%%~nxP.dll"
) 

if exist %pluginout% rmdir /s/q %pluginout%

del "%out%\*.pdb"
del "%outp%\*.pdb"

set pack="%out%\%publishName%-v%version%.zip"

echo Packaging to %pack%

mkdir "%out%\%publishName%"
mkdir "%out%\%publishName%\%dirp%"
move %out%\*.* "%out%\%publishName%\"
move %outp%\*.* "%out%\%publishName%\%dirp%"

set sz="C:\Program Files\7-Zip\7z.exe"
if not exist %sz% (
	echo 7-Zip is missing, aborting packaging
	exit /b 1
)

%sz% a %pack% .\%out%\*

echo Cleaup
rem rmdir /s/q "%out%\%publishName%"
rmdir /s/q %outp%

echo Publish complete

