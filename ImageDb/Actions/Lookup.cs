namespace ImageDb.Actions;

/// <summary>
/// Lookup an image in the tree. This will report all images where
/// the hash value is within the given tolerance. If there are no
/// images within the tolerance, the next closest image is reported.
/// </summary>
public class Lookup : ActionBase, IActionUsage
{
    public static string Usage => "lookup <file> [tolerance]";
    
    public Lookup(ArgReader args) : base(args) { }
    
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

        TryGetArg<int>(1, out var tolerance);

        LoadTree();
        Console.WriteLine($"Looking up similar entries with tolerance = {tolerance}:");
        var similar = false;
        foreach (var s in Tree.LookupAll(TreePath(file), tolerance))
        {
            Console.WriteLine(s);
            similar = true;
        }

        if (!similar)
        {
            Console.WriteLine("There were no images within the tolerance, searching for the closest match...");
            var (result, distance) = Tree.LookupDistance(TreePath(file));
            if (result == null) Console.WriteLine("The tree is empty.");
            else Console.WriteLine($"Distance: {distance}, {result}");
        }
    }
}