using ImageDb.Common;

namespace ImageDb.Actions;

/// <summary>
/// Remove an image from the database. This should be used to manually
/// repair the image folder. This does not delete the image file.
/// </summary>
public class RemoveImage : ActionBase, IActionUsage
{
    public static string Usage => "remove <file>";
    
    public RemoveImage(ArgReader args) : base(args) { }

    public bool Execute(string file)
    {
        if (!File.Exists(file))
        {
            throw new ArgumentException($"File doesn't exist \"{file}\"");
        }
        
        var removed = Tree.Remove(TreePath(file));
        if (removed) Console.WriteLine($"Removed file \"{file}\"");
        else Console.WriteLine("File was not present in database.");

        return removed;
    }
    
    public override void Execute() => Execute(GetArg(0));
}