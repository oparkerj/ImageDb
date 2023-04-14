using ImageDb.Common;
using ImageDb.Data;

namespace ImageDb.Actions;

/// <summary>
/// This is equivalent to running AddImage on every file
/// in a directory.
/// </summary>
public class IndexDirectory : IAction
{
    public static string Usage => "index <directory>";

    public ImageDbConfig Config { get; set; }

    public int Execute(string dir)
    {
        if (!Directory.Exists(dir))
        {
            throw new ArgumentException($"Cannot find directory \"{dir}\"");
        }
        
        Config.Update("Indexing directory...");
        
        var added = 0;
        using var db = new ImageDbFileHandler {Config = Config};
        foreach (var file in Directory.EnumerateFiles(dir))
        {
            var path = AddImage.Execute(file, db);
            if (path != null) added++;
        }
        
        Config.Update("Finished indexing.");
        return added;
    }
}