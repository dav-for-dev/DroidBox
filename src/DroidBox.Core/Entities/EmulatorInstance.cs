using DroidBox.Core.Enums;

namespace DroidBox.Core.Entities;

public class EmulatorInstance
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string AvdName { get; set; } = string.Empty;
    public EmulatorState State { get; set; } = EmulatorState.Stopped;
    public string AndroidVersion { get; set; } = "14.0";
    public int ApiLevel { get; set; } = 34;
    public string SystemImagePath { get; set; } = string.Empty;
    public int RamMb { get; set; } = 2048;
    public DateTime? StartedAt { get; set; }
    public int? Pid { get; set; }
}
