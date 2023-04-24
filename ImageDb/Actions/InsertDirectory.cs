using ImageDb.Common;
using ImageDb.Data;

namespace ImageDb.Actions;

public static class InsertDirectory
{
    public const int DefaultAutoDeny = -1;

    public static InsertResult Execute(string file, LookupResult lookup, int tolerance, int autoDeny, ImageDbFileHandler db)
    {
        if (lookup.Distance <= autoDeny)
        {
            db.Config.Update($"Distance: {lookup.Distance}");
            db.Config.Update("Skipping");
            return new InsertResult(file, lookup, false);
        }
        if (lookup.Distance >= tolerance)
        {
            db.Config.Update($"Distance: {lookup.Distance}");
            var added = AddImage.Execute(file, db);
            return new InsertResult(added, lookup, true);
        }
        
        db.Config.Update($"Closest distance: {lookup.Distance}, {lookup.DbPath}");
        return new InsertResult(file, lookup, null);
    }
    
    public static InsertResult PeekExecute(string file, LookupResult lookup, int tolerance, int autoDeny, ImageDbFileHandler db)
    {
        if (lookup.Distance <= autoDeny)
        {
            db.Config.Update($"Distance: {lookup.Distance}");
            db.Config.Update("Will skip");
            return new InsertResult(file, lookup, false);
        }
        if (lookup.Distance >= tolerance)
        {
            db.Config.Update($"Distance: {lookup.Distance}");
            db.Config.Update("Will add");
            return new InsertResult(null, lookup, true);
        }
        
        db.Config.Update($"Closest distance: {lookup.Distance}, {lookup.DbPath}");
        return new InsertResult(file, lookup, null);
    }
    
    /// <summary>
    /// Insert a directory of images into the tree.
    /// Results that are outside the tolerances will be automatically
    /// added or skipped.
    /// Some results will come back as neither, in which case the caller can
    /// decide whether to accept the image based on the result of the lookup.
    /// </summary>
    /// <param name="dir">Directory path.</param>
    /// <param name="tolerance">Accept tolerance. Distances greater than or equal to this value are accepted.</param>
    /// <param name="db">File handler.</param>
    /// <param name="autoDeny">Deny tolerance. Distances smaller than or equal to this are skipped.</param>
    /// <returns>Enumerable of insertion results.</returns>
    /// <exception cref="ArgumentException">If the arguments to the function are invalid.</exception>
    public static IEnumerable<InsertResult> Execute(string dir, int tolerance, ImageDbFileHandler db, int autoDeny = -1)
    {
        if (autoDeny >= tolerance)
        {
            throw new ArgumentException("Auto-deny must be less than tolerance.");
        }
        if (!Directory.Exists(dir))
        {
            throw new ArgumentException("Directory doesn't exist.");
        }

        foreach (var file in Directory.GetFiles(dir).Order(new CountOrder()))
        {
            db.Config.Update($"Checking: {ImageDbTools.DirectoryString(file)}");

            var lookup = Lookup.FindClosestResult(file, db);
            yield return Execute(file, lookup, tolerance, autoDeny, db);
        }
    }
}

/// <summary>
/// Represents the result of an insertion. This includes the file lookup
/// and tells whether the file was added or skipped or left to the receiver
/// to decide.
/// </summary>
public class InsertResult
{
    /// <summary>
    /// Path to the file being inserted.
    /// If the file was automatically accepted, this will
    /// be the path to the file after being inserted.
    /// </summary>
    public readonly string File;
    
    /// <summary>
    /// Result of looking up the file in the tree.
    /// </summary>
    public readonly LookupResult LookupResult;
    
    /// <summary>
    /// True if the file was added automatically, false if the file was
    /// skipped, and null if the receiver decides whether to add or skip.
    /// </summary>
    public readonly bool? Inserted;

    public InsertResult(string file, LookupResult lookupResult, bool? inserted)
    {
        File = file;
        LookupResult = lookupResult;
        Inserted = inserted;
    }

    /// <summary>
    /// Run the Add action on this file.
    /// This is used when Inserted is null.
    /// </summary>
    /// <param name="db">File handler.</param>
    /// <returns>Path to the file that was added.</returns>
    public string Accept(ImageDbFileHandler db) => AddImage.Execute(File, db);

    /// <inheritdoc cref="Accept(ImageDb.Data.ImageDbFileHandler)"/>
    public string Accept()
    {
        using var db = new ImageDbFileHandler {Config = LookupResult.Config};
        return Accept(db);
    }
}