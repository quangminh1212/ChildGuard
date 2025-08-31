using System.Text.Json;

namespace ChildGuard.Core.IPC;

public static class FileIpc
{
    public static void Send(string controlDir, IpcMessage msg)
    {
        Directory.CreateDirectory(controlDir);
        var file = Path.Combine(controlDir, $"{DateTime.UtcNow.Ticks}-{msg.Type}.json");
        File.WriteAllText(file, JsonSerializer.Serialize(msg));
    }

    public static void SendToService(IpcMessage msg) => Send(Paths.ControlServiceInbox, msg);
    public static void SendToTray(IpcMessage msg) => Send(Paths.ControlTrayInbox, msg);

    public static IEnumerable<IpcMessage> Receive(string controlDir)
    {
        if (!Directory.Exists(controlDir)) yield break;
        foreach (var path in Directory.GetFiles(controlDir, "*.json").OrderBy(p => p))
        {
            IpcMessage? msg = null;
            try
            {
                msg = JsonSerializer.Deserialize<IpcMessage>(File.ReadAllText(path));
            }
            catch { }
            finally { try { File.Delete(path); } catch { } }
            if (msg != null) yield return msg!;
        }
    }
}

