namespace DroidBox.Core.Enums;

/// <summary>
/// Identifies the backend runtime provider used for Android emulation.
/// </summary>
public enum RuntimeProvider
{
    /// <summary>
    /// The official Android SDK emulator.
    /// </summary>
    AndroidSdk,

    /// <summary>
    /// A QEMU-based emulation backend.
    /// </summary>
    Qemu
}
