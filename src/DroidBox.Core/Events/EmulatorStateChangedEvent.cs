using DroidBox.Core.Enums;

namespace DroidBox.Core.Events;

public record EmulatorStateChangedEvent(EmulatorState PreviousState, EmulatorState NewState, DateTime Timestamp);
