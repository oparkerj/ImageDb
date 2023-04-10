namespace ImageDb;

/// <summary>
/// Represents an action with a usage string.
/// </summary>
public interface IActionUsage
{
    /// <summary>
    /// Action usage. This value is used to identify the name of the
    /// action. The first word in the string should be the name of the
    /// action as it would be typed in a command.
    /// </summary>
    static abstract string Usage { get; }
}