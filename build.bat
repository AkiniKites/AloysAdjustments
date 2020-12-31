if exist out rmdir /s/q out
dotnet publish src\AloysAdjustments\AloysAdjustments.csproj -c Debug -r win-x64 -o out --self-contained false -p:PublishSingleFile=true
del out\*.pdb
del out\settings.json
pause