using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ChildGuard.Core.Logging;

public sealed class JsonlLogger : IAsyncDisposable
{
    private readonly Channel<object> _channel = Channel.CreateUnbounded<object>();
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _worker;
    private readonly string _logDir;
    private readonly long _maxSizeBytes;
    private readonly int _retentionDays;

    public JsonlLogger(string logDir, int retentionDays = 14, long maxSizeBytes = 200L * 1024 * 1024)
    {
        _logDir = logDir;
        _retentionDays = retentionDays;
        _maxSizeBytes = maxSizeBytes;
        Directory.CreateDirectory(_logDir);
        _worker = Task.Run(WorkerAsync);
    }

    public void Log(object evt)
    {
        _channel.Writer.TryWrite(evt);
    }

    private string CurrentLogPath()
    {
        var name = $"events-{DateTime.UtcNow:yyyyMMdd}.jsonl";
        return Path.Combine(_logDir, name);
    }

    private async Task WorkerAsync()
    {
        while (!_cts.IsCancellationRequested)
        {
            try
            {
                var evt = await _channel.Reader.ReadAsync(_cts.Token);
                var line = JsonSerializer.Serialize(evt) + "\n";
                var path = CurrentLogPath();
                await File.AppendAllTextAsync(path, line, Encoding.UTF8, _cts.Token);
                await CleanupAsync();
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                // swallow to keep logger alive
            }
        }
    }

    private async Task CleanupAsync()
    {
        // retention by days
        foreach (var file in Directory.GetFiles(_logDir, "events-*.jsonl"))
        {
            var info = new FileInfo(file);
            if (info.CreationTimeUtc < DateTime.UtcNow.AddDays(-_retentionDays))
            {
                try { File.Delete(file); } catch { }
            }
        }
        // max size rolling - if over limit, delete oldest first
        long total = 0;
        var files = Directory.GetFiles(_logDir, "events-*.jsonl")
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.CreationTimeUtc)
            .ToList();
        foreach (var f in files)
        {
            total += f.Length;
        }
        if (total > _maxSizeBytes)
        {
            foreach (var f in files.OrderBy(f => f.CreationTimeUtc))
            {
                if (total <= _maxSizeBytes) break;
                try { total -= f.Length; File.Delete(f.FullName); } catch { }
            }
        }
        await Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        try { await _worker; } catch { }
        _cts.Dispose();
    }
}

