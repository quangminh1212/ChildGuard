using System.Management;

namespace ChildGuard.Core.Monitoring;

public sealed class ProcessWatcher : IDisposable
{
    private ManagementEventWatcher? _start;
    private ManagementEventWatcher? _stop;
    public event Action<ProcessEvent>? OnProcess;

    public void Start()
    {
        _start = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
        _stop = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStopTrace"));
        _start.EventArrived += (s, e) =>
        {
            var name = e.NewEvent["ProcessName"]?.ToString() ?? "";
            var pid = Convert.ToInt32(e.NewEvent["ProcessID"]);
            OnProcess?.Invoke(new ProcessEvent(name, pid, "start"));
        };
        _stop.EventArrived += (s, e) =>
        {
            var name = e.NewEvent["ProcessName"]?.ToString() ?? "";
            var pid = Convert.ToInt32(e.NewEvent["ProcessID"]);
            OnProcess?.Invoke(new ProcessEvent(name, pid, "stop"));
        };
        _start.Start();
        _stop.Start();
    }

    public void Stop()
    {
        _start?.Stop();
        _stop?.Stop();
    }

    public void Dispose() { Stop(); _start?.Dispose(); _stop?.Dispose(); }
}

public sealed class UsbWatcher : IDisposable
{
    private ManagementEventWatcher? _watcher;
    public event Action<UsbEvent>? OnUsb;

    public void Start()
    {
        // volume change event (2=configuration changed), 3=device arrival, 4=device removal
        var query = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent");
        _watcher = new ManagementEventWatcher(query);
        _watcher.EventArrived += (s, e) =>
        {
            var drive = e.NewEvent["DriveName"]?.ToString() ?? "";
            var evtType = Convert.ToInt32(e.NewEvent["EventType"]);
            var action = evtType == 3 ? "arrival" : evtType == 4 ? "removal" : "change";
            OnUsb?.Invoke(new UsbEvent(drive, action));
        };
        _watcher.Start();
    }

    public void Stop() { _watcher?.Stop(); }
    public void Dispose() { Stop(); _watcher?.Dispose(); }
}

