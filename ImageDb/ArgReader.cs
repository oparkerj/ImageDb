namespace ImageDb;

/// <summary>
/// Wrapper for program arguments which can search for command flags.
/// </summary>
public class ArgReader
{
    public string[] Args { get; private set; }

    public ArgReader(string[] args) => Args = args;

    /// <summary>
    /// Create a set of arguments from a command string.
    ///
    /// <see cref="Split"/>
    /// </summary>
    /// <param name="cmd">Command string.</param>
    /// <returns>Split arguments.</returns>
    public static ArgReader FromCommand(string cmd) => new(Split(cmd));

    /// <summary>
    /// Split the given command. This will split the string at spaces, except
    /// for parts of the command contained within double quotes. Afterwards,
    /// the quotes will be removed. Quotes cannot be escaped for this method.
    /// </summary>
    /// <param name="command">Command string.</param>
    /// <returns>Command parts.</returns>
    private static IEnumerable<string> SplitCommand(string command)
    {
        var start = 0;
        var inside = false;
        for (var i = 0; i < command.Length; i++)
        {
            var c = command[i];
            if (c == ' ' && !inside)
            {
                if (i > start) yield return command[start..i].Replace("\"", "");
                start = i + 1;
            }
            else if (c == '"') inside = !inside;
        }
        if (start < command.Length) yield return command[start..];
    }

    /// <summary>
    /// Split a command into its parts.
    /// </summary>
    /// <param name="cmd">Command string.</param>
    /// <returns>Command parts.</returns>
    public static string[] Split(string cmd)
    {
        return SplitCommand(cmd).ToArray();
    }

    /// <summary>
    /// Searches for the option with the given name. An option is a part of
    /// the command which begins with "--".
    /// There are four formats for command values.
    /// --option
    /// --option value
    /// --option=value
    /// --option="value with spaces"
    ///
    /// In the first case, the option will have a null value.
    /// The option would also have a null value in the following situation:
    /// --option --anotherOption
    /// </summary>
    /// <param name="s">Option name.</param>
    /// <param name="value">Option value.</param>
    /// <returns>True if the option was found, false otherwise.</returns>
    private bool TryGetArgValue(string s, out string value)
    {
        value = default;
        
        for (var i = 0; i < Args.Length; i++)
        {
            var arg = Args[i];
            if (!arg.StartsWith("--")) continue;
            
            var eq = arg.IndexOf('=');
            var name = eq >= 0 ? arg.AsSpan(2, eq - 2) : arg.AsSpan(2);
            if (!name.Equals(s.AsSpan(), StringComparison.Ordinal)) continue;
            
            if (eq >= 0) value = arg[(eq + 1)..];
            else if (i < Args.Length - 1)
            {
                var next = Args[i + 1];
                value = next.StartsWith("--") ? null : next;
            }
            return true;
        }
        
        return false;
    }

    /// <summary>
    /// Get the number of arguments, including the command name.
    /// </summary>
    public int Count => Args.Length;

    /// <summary>
    /// Get the argument at the given index.
    /// </summary>
    /// <param name="i">Argument index.</param>
    public string this[int i] => Args[i];

    /// <summary>
    /// Insert the given option into the arguments.
    /// </summary>
    /// <param name="name">Option name.</param>
    /// <param name="value">Option value.</param>
    public void InsertOption(string name, string value = null)
    {
        if (value != null) name += '=' + value;
        name = "--" + name;
        Args = Args.Append(name).ToArray();
    }

    /// <summary>
    /// Check if an option is present, with or without a value.
    /// </summary>
    /// <param name="name">Option name.</param>
    /// <returns>True if the option is present, false otherwise.</returns>
    public bool HasOption(string name)
    {
        return TryGetArgValue(name, out _);
    }

    /// <summary>
    /// Get the value of the given option.
    /// </summary>
    /// <param name="name">Option name.</param>
    /// <returns>Option value.</returns>
    public string GetOption(string name)
    {
        TryGetArgValue(name, out var value);
        return value;
    }

    /// <summary>
    /// Get the option of the given value, parsed into the given type.
    /// </summary>
    /// <param name="name">Option name.</param>
    /// <typeparam name="T">Parsable option type.</typeparam>
    /// <returns>Parsed option value.</returns>
    /// <exception cref="ArgumentException">If the argument is not present or could not be
    /// parsed.</exception>
    public T GetOption<T>(string name)
        where T : IParsable<T>
    {
        var present = TryGetArgValue(name, out var value);
        if (!present)
        {
            Console.WriteLine($"Expecting value for option \"{name}\".");
            throw new ArgumentException();
        }
        present = T.TryParse(value, null, out var result);
        if (!present)
        {
            Console.WriteLine($"Expecting value of type \"{typeof(T).Name}\" for option \"{name}\".");
            throw new ArgumentException();
        }
        return result;
    }
}