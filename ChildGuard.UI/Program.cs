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
        string ui = "modern"; string? openSection = null;
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
        }
        if (ui == "classic")
        {
            var f = new Form1();
            Application.Run(f);
        }
        else
        {
            var f = new ModernMainForm();
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
