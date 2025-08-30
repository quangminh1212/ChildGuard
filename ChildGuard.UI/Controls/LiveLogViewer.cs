using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ChildGuard.Core.Configuration;

namespace ChildGuard.UI.Controls;

/// <summary>
/// Lightweight live log viewer that tails today's events-YYYYMMDD.jsonl file
/// </summary>
public class LiveLogViewer : Panel
{
    private readonly ListBox _list = new() { Dock = DockStyle.Fill, Font = new Font("Consolas", 9f) };
    private readonly Label _pathLabel = new() { Dock = DockStyle.Top, Height = 18, AutoEllipsis = true };
    private readonly Label _status = new() { Dock = DockStyle.Bottom, Height = 18, TextAlign = ContentAlignment.MiddleLeft };
    private readonly FlowLayoutPanel _toolbar = new() { Dock = DockStyle.Top, Height = 36, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };

    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 700 };
    private string _currentFile = string.Empty;
    private long _lastLength = 0;
    private bool _paused = false;
    private AppConfig _config = new();
    private string? _typeFilter = null; // null/empty = All

    private Button _btnPause = default!;
    private Button _btnOpenFolder = default!;
    private Button _btnOpenReports = default!;

    public LiveLogViewer()
    {
        BackColor = SystemColors.Window;
        Padding = new Padding(0);

        _btnPause = new Button { Text = "Pause", AutoSize = true, Margin = new Padding(0, 4, 8, 4) };
        _btnOpenFolder = new Button { Text = "Open Folder", AutoSize = true, Margin = new Padding(0, 4, 8, 4) };
        _btnOpenReports = new Button { Text = "Open Reports", AutoSize = true, Margin = new Padding(0, 4, 8, 4) };
        _btnPause.Click += (s, e) => { _paused = !_paused; _btnPause.Text = _paused ? "Resume" : "Pause"; };
        _btnOpenFolder.Click += (s, e) => { try { var dir = Path.GetDirectoryName(_currentFile); if (!string.IsNullOrWhiteSpace(dir)) System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("explorer.exe", '"' + dir + '"') { UseShellExecute = true }); } catch { } };
        _btnOpenReports.Click += (s, e) => OnOpenReports?.Invoke(this, EventArgs.Empty);

        _toolbar.Controls.AddRange(new Control[] { _btnPause, _btnOpenFolder, _btnOpenReports });
        Controls.Add(_list);
        Controls.Add(_status);
        Controls.Add(_toolbar);
        Controls.Add(_pathLabel);

        _timer.Tick += (s, e) => Tick();
    }

    public void SetTypeFilter(string? type)
    {
        _typeFilter = string.IsNullOrWhiteSpace(type) || string.Equals(type, "All", StringComparison.OrdinalIgnoreCase)
            ? null : type;
    }

    public event EventHandler? OnOpenReports;

    public void Start(AppConfig? cfg = null)
    {
        _config = cfg ?? ConfigManager.Load(out _);
        _timer.Start();
        UpdateCurrentFile(force: true);
    }

    public void Stop()
    {
        _timer.Stop();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            try { _timer.Stop(); _timer.Dispose(); } catch { }
        }
        base.Dispose(disposing);
    }

    private void Tick()
    {
        try
        {
            if (_paused) { _status.Text = "Paused"; return; }
            if (!UpdateCurrentFile()) return;

            if (!File.Exists(_currentFile)) { _status.Text = "Waiting for log..."; return; }
            var fi = new FileInfo(_currentFile);
            if (fi.Length < _lastLength) { _lastLength = 0; _list.Items.Clear(); }
            using var fs = new FileStream(_currentFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            if (fs.Length == _lastLength) { _status.Text = "Idle"; return; }
            fs.Seek(_lastLength, SeekOrigin.Begin);
            using var sr = new StreamReader(fs, Encoding.UTF8, true, 1024, leaveOpen: true);
            string? line;
            int added = 0;
            while ((line = sr.ReadLine()) != null)
            {
                if (ShouldSkipByType(line)) continue;
                added++;
                var text = FormatLine(line);
                _list.Items.Add(text);
                if (_list.Items.Count > 500)
                {
                    _list.Items.RemoveAt(0);
                }
            }
            _lastLength = fs.Position;
            if (added > 0)
            {
                _list.TopIndex = _list.Items.Count - 1;
                _status.Text = $"Updated +{added}";
            }
        }
        catch (Exception ex)
        {
            _status.Text = "Error: " + ex.Message;
        }
    }

    private bool UpdateCurrentFile(bool force = false)
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var dir = Path.Combine(_config.DataDirectory, "logs");
        var path = Path.Combine(dir, $"events-{today}.jsonl");
        if (!force && string.Equals(path, _currentFile, StringComparison.OrdinalIgnoreCase)) return true;
        _currentFile = path;
        _pathLabel.Text = path;
        _lastLength = 0;
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        return true;
    }

    private static string FormatLine(string json)
    {
        // Fast, forgiving formatting without JSON parse (avoid blocking)
        try
        {
            // Take type and timestamp roughly
            string type = TryExtract(json, "\"type\":\"");
            string ts = TryExtract(json, "\"timestamp\":\"");
            string data = TryExtract(json, "\"data\":");
            if (data.Length > 120) data = data.Substring(0, 120) + "...";
            return $"[{(DateTimeOffset.TryParse(ts, out var dto) ? dto.ToLocalTime().ToString("HH:mm:ss") : "--:--:--")}] {type} {data}";
        }
        catch { return json; }
    }

    private static string TryExtract(string s, string key)
    {
        int i = s.IndexOf(key, StringComparison.Ordinal);
        if (i < 0) return string.Empty;
        i += key.Length;
        if (key.EndsWith("\\\""))
        {
            int j = s.IndexOf('"', i);
            return j > i ? s.Substring(i, j - i) : string.Empty;
        }
        else
        {
            int j = s.IndexOf(',', i);
            if (j < 0) j = s.IndexOf('}', i);
            return j > i ? s.Substring(i, j - i) : string.Empty;
        }
    }

    private bool ShouldSkipByType(string json)
    {
        if (_typeFilter == null) return false;
        try
        {
            string type = TryExtract(json, "\"type\":\"");
            return !string.Equals(type, _typeFilter, StringComparison.OrdinalIgnoreCase);
        }
        catch { return false; }
    }

}

