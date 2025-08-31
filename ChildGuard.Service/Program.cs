using ChildGuard.Service;
using ChildGuard.Core;
using ChildGuard.Core.Config;
using ChildGuard.Core.Logging;
using ChildGuard.Core.Monitoring;
using ChildGuard.Core.Policy;

var builder = Host.CreateApplicationBuilder(args);

// Core singletons
builder.Services.AddSingleton(_ => new ConfigManager(Paths.ConfigFile));
builder.Services.AddSingleton(sp => new JsonlLogger(Paths.LogsDir,
    sp.GetRequiredService<ConfigManager>().Current.Logging.LogRetentionDays,
    sp.GetRequiredService<ConfigManager>().Current.Logging.LogMaxSizeMB * 1024L * 1024L));

// Monitors
builder.Services.AddSingleton<HookManager>();
builder.Services.AddSingleton<ActiveWindowTracker>();
builder.Services.AddSingleton<ProcessWatcher>();
builder.Services.AddSingleton<UsbWatcher>();

// Policy
builder.Services.AddSingleton<PolicyEngine>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
