using ImageDb.Common;
using ImageDb.Data;

namespace ImageDb.Actions;

/// <summary>
/// This runs the Use action on every file in a directory.
/// </summary>
public class UseAll : IAction
{
    public static string Usage => "useAll <dir>";

    public ImageDbConfig Config { get; set; }

    public static int Execute(string dir, ImageDbFileHandler db, bool autoAdd = true)
    {
        if (!Directory.Exists(dir))
        {
            throw new ArgumentException($"Cannot find directory \"{dir}\"");
        }
        
        db.Config.Update($"Using all files in \"{dir}\"");

        var added = Directory.EnumerateFiles(dir)
            .Select(file => Use.Execute(file, db, autoAdd))
            .Count(used => used);

        db.Config.Update("Finished processing files.");
        
        return added;
    }

    public int Execute(string dir)
    {
        using var db = new ImageDbFileHandler {Config = Config};
        return Execute(dir, db);
    }
}