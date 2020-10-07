param(
	[string]$connectionString,
	[string]$artifactPath,
	[string]$folderName,
	[string]$includeSubfolder,
	[string]$versionThreshold,
	[string]$migrationStrategy
)

function Get-ExePath
{
    $currentDir = (Get-Item -Path ".\").FullName

    $exeDir = Join-Path $currentDir "Console"
    $exePath = Join-Path $exeDir "SqlMigrationRunner.exe"
	
    return $exePath
}

$exePath = Get-ExePath

& "$exePath" /cs="$connectionString" /artifactPath="$artifactPath" /folderName="$folderName" /includeSubfolder="$includeSubfolder" /versionThreshold="$versionThreshold" /migrationStrategy="$migrationStrategy"

if ($LastExitCode -ne 0)
{
	Write-Error "Task failed."
	exit 1
}
