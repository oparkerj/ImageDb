using ImageDb.Data;

namespace ImageDb.Actions;

public static class InsertFile
{
    /// <summary>
    /// Lookup a file in the tree and return a handle to the result which
    /// can be used to accept the file into the tree.
    /// </summary>
    /// <param name="file">File path.</param>
    /// <param name="db">File handler.</param>
    /// <returns>Result of looking up the file in the tree.</returns>
    public static InsertResult Execute(string file, ImageDbFileHandler db)
    {
        var lookup = Lookup.FindClosestResult(file, db);
        db.Config.Update($"Closest distance: {lookup.Distance}, {lookup.DbPath}");
        return new InsertResult(file, lookup, null);
    }
}