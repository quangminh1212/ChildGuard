using ChildGuard.Core.Diagnostics;

namespace ChildGuard.Agent;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        SimpleLogger.Info("Agent starting");
        try
        {
            Application.Run(new Form1());
        }
        catch (Exception ex)
        {
            SimpleLogger.Error(ex, "Unhandled exception in Agent");
            throw;
        }
        finally
        {
            SimpleLogger.Info("Agent exiting");
        }
    }
}
