namespace ImageDb.Actions;

/// <summary>
/// Unmark an image as used.
/// </summary>
public class RemoveUse : ActionBase, IActionUsage
{
    public static string Usage => "removeUse <file>";
    
    public RemoveUse(ArgReader args) : base(args) { }

    public void Execute(string file)
    {
        if (!File.Exists(file))
        {
            throw new ArgumentException($"File doesn't exist \"{file}\"");
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
    
    public override void Execute() => Execute(GetArg(0));
}