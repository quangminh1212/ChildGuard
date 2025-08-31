namespace ChildGuard.Core.Protection;

public sealed class AudioMonitor
{
    private readonly string _ffmpegPath;
    private CancellationTokenSource? _cts;

    public event Action<double>? OnLevel; // simple RMS/peak placeholder

    public AudioMonitor(string ffmpegPath)
    {
        _ffmpegPath = ffmpegPath;
    }

    public void Start()
    {
        if (string.IsNullOrWhiteSpace(_ffmpegPath) || !File.Exists(_ffmpegPath))
            throw new FileNotFoundException("FFmpeg path not found", _ffmpegPath);
        _cts = new CancellationTokenSource();
        // Simplified placeholder: in real impl, spawn ffmpeg with silencedetect/astats and parse output
        Task.Run(async () =>
        {
            var rnd = new Random();
            while (!_cts!.IsCancellationRequested)
            {
                OnLevel?.Invoke(rnd.NextDouble());
                await Task.Delay(1000, _cts.Token);
            }
        }, _cts.Token);
    }

    public void Stop() { _cts?.Cancel(); }
}

