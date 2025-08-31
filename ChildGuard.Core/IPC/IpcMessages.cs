namespace ChildGuard.Core.IPC;

public record IpcMessage(string Type, object Payload);
public record EnforcementSnoozeRequest(string ProcessName, int Minutes);
public record ToastAlert(string Title, string Message, int CountdownSeconds, string? Url = null, string? Process = null, string? Rule = null, System.DateTime? DeadlineUtc = null);


