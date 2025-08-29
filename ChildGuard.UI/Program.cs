namespace ChildGuard.UI;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        string ui = "windows"; string? openSection = null; bool debugUi = false;
        var args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--open", StringComparison.OrdinalIgnoreCase) && i+1 < args.Length)
            {
                var v = args[i+1].ToLowerInvariant();
                openSection = v switch {
                    "settings" => "Settings",
                    "reports" => "Reports",
                    "dashboard" => "Dashboard",
                    "monitoring" => "Monitoring",
                    "protection" => "Protection",
                    _ => openSection
                };
                i++;
            }
            else if (string.Equals(args[i], "--ui", StringComparison.OrdinalIgnoreCase) && i+1 < args.Length)
            {
                ui = args[i+1].ToLowerInvariant();
                i++;
            }
            else if (string.Equals(args[i], "--debug-ui", StringComparison.OrdinalIgnoreCase))
            {
                debugUi = true;
            }
            else if (string.Equals(args[i], "--demo", StringComparison.OrdinalIgnoreCase))
            {
                // Show Fluent Design Demo
                Application.Run(new FluentDemoForm());
                return;
            }
            else if (string.Equals(args[i], "--modern-demo", StringComparison.OrdinalIgnoreCase))
            {
                // Show Modern UI Demo (previous version)
                Application.Run(new ModernDemoForm());
                return;
            }
            else if (string.Equals(args[i], "--material-demo", StringComparison.OrdinalIgnoreCase))
            {
                // Show Material Design Demo (old)
                Application.Run(new MaterialDemoForm());
                return;
            }
        }
        if (ui == "classic")
        {
            var f = new Form1();
            Application.Run(f);
        }
        else if (ui == "modern")
        {
            var f = new ModernMainForm();
            f.Tag = debugUi ? "DEBUG_UI" : null;
            f.Shown += (s, e) =>
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(openSection)) f.NavigateTo(openSection);
                }
                catch { }
            };
            Application.Run(f);
        }
        else // windows (default)
        {
            var f = new WindowsMainForm();
            f.Shown += (s, e) =>
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(openSection)) f.NavigateTo(openSection);
                }
                catch { }
            };
            Application.Run(f);
        }
    }
}
