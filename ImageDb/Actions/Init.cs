using ImageDb.Common;
using ImageDb.Data;

namespace ImageDb.Actions;

/// <summary>
/// Create default configuration files.
/// This will create a config file, an empty database, an
/// empty "used" file, and create the default image folder.
/// </summary>
public class Init : IAction
{
    public static string Usage => "init";

    public ImageDbConfig Config { get; set; }

    public void Execute()
    {
        using var db = new ImageDbFileHandler {Config = Config};
        
        db.LoadTree();
        db.LoadUsage();
        db.TreeUpdated = true;
        db.UsageUpdated = true;

        Directory.CreateDirectory(Config.ImageFolderRelative);
        Config.SaveConfigFile();
    }
}