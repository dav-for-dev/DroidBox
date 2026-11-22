namespace DroidBox.Core.Events;

public record RuntimeErrorEvent(string Message, Exception? Exception, DateTime Timestamp);
