[CmdletBinding()]
param (
)

$project = Get-Project

$fodyWeaversPath = [System.IO.Path]::Combine([System.IO.Path]::GetDirectoryName($project.FullName), "FodyWeavers.xml")

$toolsDir = Split-Path -Parent $MyInvocation.MyCommand.Path

$configExe = Join-Path $toolsDir "Config.exe"

& $configExe $fodyWeaversPath
