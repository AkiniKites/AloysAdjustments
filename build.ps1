
$version = '1.0.0'
if ($args.count -gt 0) {
    $version = $args[0];
}

function Build-Project {
    param (
        [string] $path,
        [switch] $Rebuild
    )

    Remove-Item $tmp -Recurse -Force -ErrorAction Ignore

    Write-Host "Building $path"

    $target = 'BeforeBuild,Build'
    if ($Rebuild) { $target = 'Clean,BeforeBuild,Build'}

    $buildResult = Invoke-MsBuild -Path $path -MsBuildParameters "/t:$target /p:Configuration=Release /m /p:OutputPath=""$tmp"" /p:Version=$version" -ShowBuildOutputInCurrentWindow
    if ($buildResult.BuildSucceeded -eq $true) {
        Write-Host ("Build completed successfully in {0:N1} seconds." -f $buildResult.BuildDuration.TotalSeconds)
    }
    elseif ($buildResult.BuildSucceeded -eq $false) {
        Write-Host ("Build failed after {0:N1} seconds. Check the build log file '$($buildResult.BuildLogFilePath)' for errors." -f $buildResult.BuildDuration.TotalSeconds)
    }
    elseif ($null -eq $buildResult.BuildSucceeded) {
        Write-Host "Unsure if build passed or failed: $($buildResult.Message)"
    }

    return ($buildResult.BuildSucceeded -eq $true)
}

Install-Module Invoke-MsBuild

$publishName="Aloy's Adjustments";
$out='out'
$publishDir="$out\$publishName"

$plugins='plugins'
$publishPlugins="$publishDir\$plugins"

$tmp=$ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath("$out\tmp")

Write-Host "Building with version $version to $out"

Remove-Item $out -Recurse -Force -ErrorAction Ignore
New-Item -ItemType Directory -Path $out

New-Item -ItemType Directory -Path $publishDir
New-Item -ItemType Directory -Path $publishPlugins

Write-Host "Building main project..."
if ($(Build-Project 'src\AloysAdjustments\AloysAdjustments.csproj' -Rebuild) -ne $true) {
    exit
}

Move-Item "$tmp\AloysAdjustments.exe" "$publishDir\$publishName.exe"
Move-Item "$tmp\config.json" "$publishDir\config.json"

Write-Host "Building plugins..."
foreach ($project in $(dir 'src\Plugins' | ?{$_.PSISContainer})) {
    $projFile = "$([IO.Path]::Combine($project.FullName, $project.Name)).csproj"
    if ($(Build-Project $projFile) -ne $true) {
        exit
    }
    
    Move-Item "$tmp\$($project.Name).dll" "$publishPlugins\$($project.Name).dll"
}

Write-Host "Cleanup..."
Remove-Item $tmp -Recurse -Force -ErrorAction Ignore

Write-Host "Compressing..."
$sz="C:\Program Files\7-Zip\7z.exe"
if ((Test-Path $sz) -eq $false) {
	Write-Host "7-Zip is missing, aborting packaging"
    exit
}

$pack="$out\$publishName-v$version.zip"

& $sz a $pack .\$out\*

#Compress-Archive -Path $publishDir -DestinationPath "$out\$publishName.zip"