using ImageDb.Common;
using ImageDb.Data;

namespace ImageDb.Actions;

/// <summary>
/// Manually mark an image as used. If the image does not exist in the database,
/// it will first be added, which will cause the file to be moved to the image
/// folder.
/// </summary>
public class Use : IAction
{
    public static string Usage => "use <file>";

    public ImageDbConfig Config { get; set; }

    public static bool Execute(string file, ImageDbFileHandler db, bool autoAdd = true)
    {
        if (!File.Exists(file))
        {
            throw new ArgumentException($"File doesn't exist \"{file}\"");
        }

        // File path relative to image folder
        var fileRelative = db.Config.RelativeToImageFolder(file);
        
        if (!db.Tree.Contains(fileRelative))
        {
            db.Config.Update("Image is not indexed.");
            if (autoAdd)
            {
                file = AddImage.Execute(file, db);
                fileRelative = db.Config.RelativeToImageFolder(file);
            }
            else
            {
                db.Config.Update("Skipping...");
                return false;
            }
        }

        var success = db.AddUsage(fileRelative);

        if (success)
        {
            db.Config.Update($"Marked \"{fileRelative}\" as used.");
        }
        else
        {
            db.Config.Update($"File \"{fileRelative}\" has already been used.");
        }

        return success;
    }

    public bool Execute(string file)
    {
        using var db = new ImageDbFileHandler {Config = Config};
        return Execute(file, db);
    }
}