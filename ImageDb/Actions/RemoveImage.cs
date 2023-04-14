using ImageDb.Common;
using ImageDb.Data;

namespace ImageDb.Actions;

/// <summary>
/// Remove an image from the database. This should be used to manually
/// repair the image folder. This does move the image from the image folder.
/// </summary>
public class RemoveImage : IAction
{
    public static string Usage => "remove <file>";

    public ImageDbConfig Config { get; set; }

    public static bool Execute(string file, ImageDbFileHandler db)
    {
        if (!File.Exists(file))
        {
            throw new ArgumentException($"File doesn't exist \"{file}\"");
        }
        
        var removed = db.RemoveImage(file);

        if (removed)
        {
            db.Config.Update($"Removed file \"{file}\"");
        }
        else
        {
            db.Config.Update("File was not present in database.");
        }

        return removed;
    }

    public bool Execute(string file)
    {
        using var db = new ImageDbFileHandler {Config = Config};
        return Execute(file, db);
    }
}