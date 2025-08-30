param(
  [string]$ResultsDir = "TestResults",
  [string]$ReportDir = "CoverageReport"
)

# Requires dotnet-reportgenerator-globaltool
# Install locally if missing (does not modify global machine state)
$toolPath = "$PSScriptRoot\.tools"
$env:PATH = "$toolPath;" + $env:PATH

if (-not (Get-Command reportgenerator -ErrorAction SilentlyContinue)) {
  dotnet tool install dotnet-reportgenerator-globaltool --tool-path $toolPath | Out-Null
}

$coverage = Get-ChildItem -Recurse -Filter coverage.cobertura.xml -Path $ResultsDir | Select-Object -First 1
if (-not $coverage) {
  Write-Host "No coverage report found under $ResultsDir" -ForegroundColor Yellow
  exit 0
}

reportgenerator -reports:$coverage.FullName -targetdir:$ReportDir -reporttypes:HtmlInline_AzurePipelines;Cobertura
Write-Host "Coverage HTML generated at $ReportDir/index.html"

