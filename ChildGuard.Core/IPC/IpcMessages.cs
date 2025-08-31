namespace ChildGuard.Core.IPC;

public record IpcMessage(string Type, object Payload);
public record EnforcementSnoozeRequest(string ProcessName, int Minutes);

