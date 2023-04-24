using ImageDb.Common;
using ImageDb.Data;

namespace ImageDb.Actions;

/// <summary>
/// Lookup an image in the tree. This will report all images where
/// the hash value is within the given tolerance. If there are no
/// images within the tolerance, the next closest image is reported.
/// </summary>
public class Lookup : IAction
{
    public static string Usage => "lookup <file> [tolerance]";

    public ImageDbConfig Config { get; set; }

    public static IEnumerable<string> Find(string file, int tolerance, ImageDbFileHandler db)
    {
        if (!File.Exists(file))
        {
            throw new ArgumentException($"File doesn't exist \"{file}\"");
        }
        
        foreach (var similar in db.Tree.LookupAll(db.Config.RelativeToImageFolder(file), tolerance))
        {
            db.Config.Update(similar);
            yield return similar;
        }
    }

    public static IEnumerable<string> FindRelative(string file, int tolerance, ImageDbFileHandler db)
    {
        return Find(file, tolerance, db).FromRelativePaths(db.Config.ImageFolderRelative);
    }

    public static string FindOne(string file, int tolerance, ImageDbFileHandler db)
    {
        return Find(file, tolerance, db).FirstOrDefault();
    }

    public static string FindOneRelative(string file, int tolerance, ImageDbFileHandler db)
    {
        return FindRelative(file, tolerance, db).FirstOrDefault();
    }

    public static (string Path, int Distance) FindClosest(string file, ImageDbFileHandler db)
    {
        if (!File.Exists(file))
        {
            throw new ArgumentException($"File doesn't exist \"{file}\"");
        }

        return db.Tree.LookupDistance(db.Config.RelativeToImageFolder(file));
    }

    public static LookupResult FindClosestResult(string file, ImageDbFileHandler db)
    {
        var (path, distance) = FindClosest(file, db);
        return new LookupResult(path, distance) {Config = db.Config};
    }
    
    public static LookupResult FindClosestResult(long hash, ImageDbFileHandler db)
    {
        var (path, distance) = db.Tree.LookupDistance(hash);
        return new LookupResult(path, distance) {Config = db.Config};
    }

    public static List<string> Execute(string file, int tolerance, bool fallbackClosest, ImageDbFileHandler db)
    {
        db.Config.Update($"Looking up similar entries with tolerance = {tolerance}:");
        using var similar = Find(file, tolerance, db).GetEnumerator();

        if (similar.MoveNext())
        {
            // Enumerating calls Config.Update
            var result = new List<string> {similar.Current};
            while (similar.MoveNext())
            {
                result.Add(similar.Current);
            }
            return result;
        }
        
        if (fallbackClosest)
        {
            db.Config.Update("There were no images within the tolerance, searching for the closest match...");
            var (result, distance) = db.Tree.LookupDistance(db.Config.RelativeToImageFolder(file));
            if (result == null)
            {
                db.Config.Update("The tree is empty.");
            }
            else
            {
                db.Config.Update($"Distance: {distance}, {result}");
                return new List<string>(1) {result};
            }
        }
        else
        {
            db.Config.Update("There were no images within the tolerance.");
        }

        return new List<string>(0);
    }

    public List<string> Execute(string file, int tolerance = 0, bool fallbackClosest = true)
    {
        using var db = new ImageDbFileHandler {Config = Config};
        return Execute(file, tolerance, fallbackClosest, db);
    }
}

public record LookupResult(string DbPath, int Distance)
{
    public required ImageDbConfig Config { get; init; }
}