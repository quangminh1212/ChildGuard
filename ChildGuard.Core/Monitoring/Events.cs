namespace ChildGuard.Core.Monitoring;

public record BaseEvent(string Type, DateTime TimestampUtc, object Data);

public record KeyEvent(string Key, bool IsDown);
public record MouseEvent(string Button, int X, int Y, string Action);
public record ActiveWindowEvent(string ProcessName, string WindowTitle);
public record ProcessEvent(string ProcessName, int Pid, string Action);
public record UsbEvent(string DriveLetter, string Action);

