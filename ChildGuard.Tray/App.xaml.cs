using System.Windows;

namespace ChildGuard.Tray;

public partial class App : Application
{
    private TrayApp? _tray;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        // Start headless tray app
        _tray = new TrayApp();
        // Hide main window (not used in this minimal tray mode)
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _tray?.Dispose();
        base.OnExit(e);
    }
}
