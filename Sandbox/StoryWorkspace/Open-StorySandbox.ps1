param(
    [switch]$PrepareOnly,
    [string]$Unity = 'C:/Program Files/Unity/Hub/Editor/6000.3.24f1/Editor/Unity.exe'
)
$ErrorActionPreference = 'Stop'
$repo = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$target = Join-Path $repo '.sandbox/StoryWorkspaceProject'
$source = Join-Path $repo 'Project Love'
$marker = Join-Path $target 'ProjectSettings/ProjectLoveStorySandbox.txt'
if (-not (Test-Path -LiteralPath $marker)) {
    if (Test-Path -LiteralPath $target) { throw 'Sandbox folder exists without its marker. Inspect it before preparing.' }
    New-Item -ItemType Directory -Path $target | Out-Null
    foreach ($part in @('Assets','Packages','ProjectSettings')) {
        Copy-Item -LiteralPath (Join-Path $source $part) -Destination (Join-Path $target $part) -Recurse
    }
    Set-Content -LiteralPath $marker -Value 'Isolated story authoring UX trial. Never apply to the production project automatically.'
    Write-Output 'Created an isolated copy of the current campaign.'
} else {
    Write-Output 'Preserving existing sandbox content and drafts. Updating sandbox tool code only.'
}
$toolAssets = Join-Path $target 'Assets/_Project/Sandbox/StoryWorkspace'
New-Item -ItemType Directory -Path $toolAssets -Force | Out-Null
foreach ($part in @('Editor','Tests')) {
    $destination = Join-Path $toolAssets $part
    New-Item -ItemType Directory -Path $destination -Force | Out-Null
    Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot $part) -File | ForEach-Object {
        Copy-Item -LiteralPath $_.FullName -Destination (Join-Path $destination $_.Name) -Force
    }
}
Write-Output ('Sandbox project: ' + $target)
if (-not $PrepareOnly) {
    if (-not (Test-Path -LiteralPath $Unity)) { throw 'Unity 6000.3.24f1 was not found. Pass -Unity with its executable path.' }
    $editorLog = Join-Path $target 'Logs/StorySandboxEditor.log'
    New-Item -ItemType Directory -Path (Join-Path $target 'Logs') -Force | Out-Null
    Start-Process -FilePath $Unity -ArgumentList @('-projectPath', ('"' + $target + '"'), '-executeMethod', 'ProjectLove.StorySandbox.StoryWorkspaceWindow.Open', '-logFile', ('"' + $editorLog + '"')) -WindowStyle Hidden | Out-Null
}
