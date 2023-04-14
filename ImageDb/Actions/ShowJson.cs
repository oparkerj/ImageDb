using ImageDb.Common;
using ImageDb.Data;

namespace ImageDb.Actions;

/// <summary>
/// Write plain and neatly indented versions of the config files.
/// </summary>
public class ShowJson : IAction
{
    public static string Usage => "showjson";

    public ImageDbConfig Config { get; set; }

    public void Execute()
    {
        using var db = new ImageDbFileHandler {Config = Config};

        var oldSetting = db.Config.ShowJson;
        db.Config.ShowJson = true;
        
        db.LoadTree();
        db.TreeUpdated = true;
        db.SaveTree();
        
        db.LoadUsage();
        db.UsageUpdated = true;
        db.SaveUsage();

        db.Config.ShowJson = oldSetting;
    }
}