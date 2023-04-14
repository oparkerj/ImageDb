using ImageDb.Common;
using ImageDb.Data;

namespace ImageDb.Actions;

/// <summary>
/// Unmark an image as used.
/// </summary>
public class RemoveUse : IAction
{
    public static string Usage => "removeUse <file>";

    public ImageDbConfig Config { get; set; }

    public static bool Execute(string file, ImageDbFileHandler db)
    {
        if (!File.Exists(file))
        {
            throw new ArgumentException($"File doesn't exist \"{file}\"");
        }
        
        var removed = db.RemoveUsage(db.Config.RelativeToImageFolder(file));
        
        if (removed)
        {
            db.Config.Update("Removed file from used.");
        }
        else
        {
            db.Config.Update("File is not used.");
        }

        return removed;
    }

    public bool Execute(string file)
    {
        using var db = new ImageDbFileHandler {Config = Config};
        return Execute(file, db);
    }
}