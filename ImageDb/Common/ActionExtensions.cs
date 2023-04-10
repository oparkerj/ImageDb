using ImageDb.Actions;

namespace ImageDb.Common;

public static class ActionExtensions
{
    /// <summary>
    /// Load the "used" configuration file.
    /// This file stores which images have been marked as used.
    /// The file to load can be overridden with the --usefile option.
    /// </summary>
    /// <param name="action">Current action.</param>
    /// <returns>Used files.</returns>
    public static HashSet<string> LoadUseFile(this ActionBase action)
    {
        var useFile = action.UseFile;
        return action.Data.ReadGZipped<HashSet<string>>(useFile);
    }

    /// <summary>
    /// Save the used file back to disk.
    /// </summary>
    /// <param name="action">Current action.</param>
    /// <param name="used">Used files.</param>
    public static void WriteUseFile(this ActionBase action, HashSet<string> used)
    {
        var useFile = action.UseFile; 
        action.Data.WriteGZipped(used, useFile, action.Args.HasOption(ShowJson.Usage));
    }

    /// <summary>
    /// Move the given file to the configured image folder.
    /// The file will be renamed according to the configured file name format.
    ///
    /// <seealso cref="Use.NextFileName"/>
    /// </summary>
    /// <param name="action">Current action.</param>
    /// <param name="file">File path.</param>
    /// <returns>Path to the moved and renamed file.</returns>
    public static string MoveFileToDir(this ActionBase action, string file)
    {
        var dir = action.ImageDirPath;
        var ext = Path.GetExtension(file);
        var name = Use.NextFileName(dir, action.Config.NameFormat);
        name = name.Replace("{ext}", ext);
        var dest = Path.Combine(dir, name);
        File.Move(file, dest);
        return dest;
    }

    /// <summary>
    /// Check if a usage string is for the given command.
    /// This compares the command name from the usage to the given
    /// command name.
    /// </summary>
    /// <param name="usage">Usage string.</param>
    /// <param name="cmd">Command name.</param>
    /// <returns>True if the usage starts with the given command name,
    /// false otherwise.</returns>
    public static bool IsCommand(this string usage, string cmd)
    {
        var space = usage.IndexOf(' ');
        if (space < 0) return usage == cmd;
        return usage.AsSpan(0, space).Equals(cmd.AsSpan(), StringComparison.Ordinal);
    }

    /// <summary>
    /// Takes a list of paths within the image directory and prepends the image
    /// directory path.
    /// </summary>
    /// <param name="list">Image paths.</param>
    /// <param name="imageDir">Image directory path.</param>
    /// <returns>Input list.</returns>
    public static List<string> RelativeFromImageDir(this List<string> list, string imageDir)
    {
        for (var i = 0; i < list.Count; i++)
        {
            list[i] = Path.Join(imageDir, list[i]);
        }
        return list;
    }
}