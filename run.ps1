param(
  [switch]$Diagnose,
  [string]$Ui = "windows",
  [string]$Open = ""
)

$ErrorActionPreference = 'SilentlyContinue'

# Stop existing UI to avoid file locks
$uiProc = Get-Process -Name "ChildGuard.UI" -ErrorAction SilentlyContinue
if ($uiProc) {
  Write-Host "[INFO] Stopping existing ChildGuard.UI.exe ($($uiProc.Id))..."
  $uiProc | Stop-Process -Force -ErrorAction SilentlyContinue
  Start-Sleep -Milliseconds 500
}

# Compose args
$uiArgs = @()
if ($Ui) { $uiArgs += @('--ui', $Ui) }
if ($Open) { $uiArgs += @('--open', $Open) }

# Prefer built exe
$exePath = Join-Path $PSScriptRoot 'ChildGuard.UI\bin\Debug\net8.0-windows\ChildGuard.UI.exe'
if (Test-Path $exePath) {
  if ($Diagnose) {
    Write-Host "[DIAG] $exePath $($uiArgs -join ' ')"
    & $exePath @uiArgs
  } else {
    Start-Process -FilePath $exePath -ArgumentList ($uiArgs -join ' ') -WorkingDirectory $PSScriptRoot | Out-Null
  }
} else {
  if ($Diagnose) {
    Write-Host "[DIAG] dotnet run --project ChildGuard.UI -- $($uiArgs -join ' ')"
    dotnet run --project ChildGuard.UI -- @uiArgs
  } else {
    Start-Process -FilePath 'dotnet' -ArgumentList ("run --project ChildGuard.UI -- " + ($uiArgs -join ' ')) -WorkingDirectory $PSScriptRoot | Out-Null
  }
}

