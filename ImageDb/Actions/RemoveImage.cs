namespace ImageDb.Actions;

/// <summary>
/// Remove an image from the database. This should be used to manually
/// repair the image folder. This does not delete the image file.
/// </summary>
public class RemoveImage : ActionBase, IActionUsage
{
    public static string Usage => "remove <file>";
    
    public RemoveImage(ArgReader args) : base(args) { }
    
    public override void Execute()
    {
        if (!GetArgs(1, out var args))
        {
            Console.WriteLine(Usage);
            return;
        }
        
        var file = args[0];
        if (!File.Exists(file))
        {
            Console.WriteLine($"File doesn't exist \"{file}\"");
            return;
        }
        
        LoadTree();
        var removed = Tree.Remove(TreePath(file));
        if (removed) Console.WriteLine($"Removed file \"{file}\"");
        else Console.WriteLine("File was not present in database.");
    }
}