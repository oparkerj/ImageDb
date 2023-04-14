using ImageDb.Common;
using ImageDb.Data;

namespace ImageDb.Actions;

/// <summary>
/// Print out the list of files marked as used.
/// </summary>
public class ShowUsed : IAction
{
    public static string Usage => "showUsed";

    public ImageDbConfig Config { get; set; }

    private static IEnumerable<string> ExecuteInternal(ImageDbFileHandler db)
    {
        return db.Used.Order(new CountOrder());
    }

    public static List<string> ExecuteRelative(ImageDbFileHandler db)
    {
        return ExecuteInternal(db).FromRelativePaths(db.Config.ImageFolderRelative).ToList();
    }

    public static List<string> Execute(ImageDbFileHandler db)
    {
        return ExecuteInternal(db).ToList();
    }

    public List<string> Execute()
    {
        using var db = new ImageDbFileHandler {Config = Config};

        var used = Execute(db);
        
        if (used.Count == 0)
        {
            Config.Update("No files are used.");
        }
        else
        {
            foreach (var use in used)
            {
                Config.Update(use);
            }
        }

        return used;
    }
}