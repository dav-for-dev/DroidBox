using DroidBox.Core.Entities;

namespace DroidBox.Core.Events;

public record ApkInstalledEvent(ApkInfo ApkInfo, DateTime Timestamp);
