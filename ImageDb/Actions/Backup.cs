using ImageDb.Common;
using ImageDb.Data;

namespace ImageDb.Actions;

/// <summary>
/// This action can create backups for the database and usage files.
/// </summary>
public class Backup : IAction
{
    public static string Usage => "backup (used | db)";
    
    public ImageDbConfig Config { get; set; }

    public string Execute(string which = null)
    {
        using var db = new ImageDbFileHandler {Config = Config};
        
        if (which == "used")
        {
            // This one writes plain JSON instead of GZip.
            var used = db.Used.Order(new CountOrder()).ToList();
            var output = Path.ChangeExtension(db.Config.UsageFileRelative, ".bak");
            db.Config.FileHandler.WriteJson(used, output);
            db.Config.Update($"Saved to: {ImageDbTools.DirectoryString(output)}");
            return output;
        }
        
        if (which == "db")
        {
            var output = Path.ChangeExtension(db.Config.DatabaseRelative, ".bak");
            db.Config.FileHandler.WriteTree(db.Tree, output);
            db.Config.Update($"Saved to: {ImageDbTools.DirectoryString(output)}");
            return output;
        }
        
        throw new ArgumentException("Invalid target.");
    }
}