using ImageDb.Common;
using ImageDb.Data;

namespace ImageDb.Actions;

/// <summary>
/// This action adds an image to the database.
/// The file will be moved into the image folder.
/// This does not check if a similar image already exists.
/// </summary>
public class AddImage : IAction
{
    public static string Usage => "add <file>";

    public ImageDbConfig Config { get; set; }

    public static string Execute(string file, ImageDbFileHandler db)
    {
        if (!File.Exists(file))
        {
            throw new ArgumentException($"File doesn't exist \"{file}\"");
        }
        
        db.Config.Update($"Adding: {ImageDbTools.DirectoryString(file)}");

        var moved = ImageDbTools.MoveFileToImageDir(file, db.Config);
        var success = db.TryAddImage(moved);

        return success ? moved : null;
    }

    public string Execute(string file)
    {
        using var db = new ImageDbFileHandler {Config = Config};
        return Execute(file, db);
    }
}