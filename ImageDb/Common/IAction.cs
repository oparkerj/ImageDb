namespace ImageDb.Common;

/// <summary>
/// Represents an action that the library can perform.
/// </summary>
public interface IAction
{
    /// <summary>
    /// Action usage. This value should display the syntax for
    /// the action if it were typed on the command line.
    /// </summary>
    static abstract string Usage { get; }
    
    /// <summary>
    /// The configuration to use while executing the command.
    /// </summary>
    ImageDbConfig Config { get; set; }
}