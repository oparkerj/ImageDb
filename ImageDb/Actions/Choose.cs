using ImageDb.Common;
using ImageDb.Data;

namespace ImageDb.Actions;

/// <summary>
/// Select an image from the database to mark as "used".
/// Images marked as used will not be chosen again.
/// </summary>
public class Choose : IAction
{
    public static string Usage => "choose";

    public ImageDbConfig Config { get; set; }

    public string Execute()
    {
        using var db = new ImageDbFileHandler {Config = Config};

        var chosen = db.Tree.Except(db.Used).FirstOrDefault();
        if (chosen == null)
        {
            Config.Update("There are no unused images.");
            return null;
        }

        db.AddUsage(chosen);
        // Save here, so any issue will come up in an exception
        // before reporting a chosen image.
        db.SaveUsage();
        
        Config.Update("Chosen image:");
        Config.Update(chosen);

        return Path.Join(Config.ImageFolderRelative, chosen);
    }
}