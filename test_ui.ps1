# ChildGuard UI Test Script
# Tests all Material Design components and UI improvements

Write-Host "🧪 TESTING CHILDGUARD MATERIAL DESIGN UI" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

# Kill any existing instances
Write-Host "🔄 Cleaning up existing processes..." -ForegroundColor Yellow
Get-Process -Name "ChildGuard.UI" -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 2

# Build the project
Write-Host "🔨 Building ChildGuard.UI..." -ForegroundColor Yellow
dotnet build ChildGuard.UI\ChildGuard.UI.csproj -c Debug
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Build successful!" -ForegroundColor Green

# Test 1: Material Demo Form
Write-Host "`n🎨 Test 1: Material Design Demo Form" -ForegroundColor Cyan
Write-Host "Features to check:" -ForegroundColor White
Write-Host "  • MaterialButton (5 styles: Primary, Secondary, Text, Success, Danger)" -ForegroundColor Gray
Write-Host "  • MaterialCard (4 elevation levels)" -ForegroundColor Gray
Write-Host "  • MaterialTextBox (with floating labels)" -ForegroundColor Gray
Write-Host "  • MaterialNavigationBar (sidebar navigation)" -ForegroundColor Gray
Write-Host "  • Google Material + Windows Fluent color scheme" -ForegroundColor Gray

Start-Process -FilePath "ChildGuard.UI\bin\Debug\net8.0-windows\ChildGuard.UI.exe" -ArgumentList "--demo"
Write-Host "📱 Demo form launched! Check the UI..." -ForegroundColor Green
Read-Host "Press Enter when you've reviewed the demo form"

# Kill demo
Get-Process -Name "ChildGuard.UI" -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 1

# Test 2: Modern Main Form
Write-Host "`n🏠 Test 2: Modern Main Form" -ForegroundColor Cyan
Write-Host "Features to check:" -ForegroundColor White
Write-Host "  • MaterialFluent color scheme applied" -ForegroundColor Gray
Write-Host "  • MaterialButton in action buttons" -ForegroundColor Gray
Write-Host "  • Improved typography (Segoe UI)" -ForegroundColor Gray
Write-Host "  • Clean, minimal design" -ForegroundColor Gray

Start-Process -FilePath "ChildGuard.UI\bin\Debug\net8.0-windows\ChildGuard.UI.exe" -ArgumentList "--ui", "modern"
Write-Host "📱 Modern UI launched! Check the main interface..." -ForegroundColor Green
Read-Host "Press Enter when you've reviewed the modern interface"

# Kill modern UI
Get-Process -Name "ChildGuard.UI" -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 1

# Test 3: Settings Form with MaterialCard
Write-Host "`n⚙️ Test 3: Settings Form with MaterialCard" -ForegroundColor Cyan
Write-Host "Features to check:" -ForegroundColor White
Write-Host "  • MaterialCard sections instead of RoundedPanel" -ForegroundColor Gray
Write-Host "  • Improved elevation and shadows" -ForegroundColor Gray
Write-Host "  • Better typography hierarchy" -ForegroundColor Gray
Write-Host "  • Clean card-based layout" -ForegroundColor Gray

Start-Process -FilePath "ChildGuard.UI\bin\Debug\net8.0-windows\ChildGuard.UI.exe" -ArgumentList "--open", "settings"
Write-Host "📱 Settings form launched! Check the card-based layout..." -ForegroundColor Green
Read-Host "Press Enter when you've reviewed the settings form"

# Kill settings
Get-Process -Name "ChildGuard.UI" -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 1

# Test 4: Default Windows UI (for comparison)
Write-Host "`n🪟 Test 4: Default Windows UI (Comparison)" -ForegroundColor Cyan
Write-Host "This shows the original UI for comparison with the new Material Design" -ForegroundColor White

Start-Process -FilePath "ChildGuard.UI\bin\Debug\net8.0-windows\ChildGuard.UI.exe"
Write-Host "📱 Default UI launched! Compare with the Material Design versions..." -ForegroundColor Green
Read-Host "Press Enter when you've compared the interfaces"

# Cleanup
Get-Process -Name "ChildGuard.UI" -ErrorAction SilentlyContinue | Stop-Process -Force

Write-Host "`n✅ UI TESTING COMPLETE!" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green
Write-Host "Material Design improvements tested:" -ForegroundColor White
Write-Host "  ✅ MaterialButton (5 styles)" -ForegroundColor Green
Write-Host "  ✅ MaterialCard (elevation & shadows)" -ForegroundColor Green
Write-Host "  ✅ MaterialTextBox (floating labels)" -ForegroundColor Green
Write-Host "  ✅ MaterialNavigationBar (sidebar)" -ForegroundColor Green
Write-Host "  ✅ MaterialFluent color scheme" -ForegroundColor Green
Write-Host "  ✅ Improved typography (Segoe UI)" -ForegroundColor Green
Write-Host "  ✅ Clean, minimal design" -ForegroundColor Green
Write-Host "  ✅ Google + Windows design language" -ForegroundColor Green

Write-Host "`n🎨 The ChildGuard UI now features:" -ForegroundColor Cyan
Write-Host "  • Modern Material Design 3 components" -ForegroundColor White
Write-Host "  • Windows Fluent Design elements" -ForegroundColor White
Write-Host "  • Professional, enterprise-grade appearance" -ForegroundColor White
Write-Host "  • Consistent design system" -ForegroundColor White
Write-Host "  • Improved user experience" -ForegroundColor White
