namespace ImageDb.Actions;

/// <summary>
/// Unmark an image as used.
/// </summary>
public class RemoveUse : ActionBase, IActionUsage
{
    public static string Usage => "removeUse <file>";
    
    public RemoveUse(ArgReader args) : base(args) { }
    
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

        var used = this.LoadUseFile();
        used ??= new HashSet<string>();
        var removed = used.Remove(TreePath(file));
        if (removed)
        {
            Console.WriteLine("Removed file from used.");
            this.WriteUseFile(used);
            Console.WriteLine("Saved use file.");
        }
        else
        {
            Console.WriteLine("File is not used.");
        }
    }
}