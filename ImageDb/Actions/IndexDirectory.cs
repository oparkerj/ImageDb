namespace ImageDb.Actions;

/// <summary>
/// This is equivalent to running "add [file]" on every file
/// in a directory.
/// </summary>
public class IndexDirectory : ActionBase, IActionUsage
{
    public static string Usage => "index <directory>";
    
    public IndexDirectory(ArgReader args) : base(args) { }
    
    public override void Execute()
    {
        if (!GetArgs(1, out var args))
        {
            Console.WriteLine(Usage);
            return;
        }

        var dir = args[0];
        if (!Directory.Exists(dir))
        {
            Console.WriteLine($"Cannot find directory \"{dir}\"");
            return;
        }
        
        Console.WriteLine("Indexing directory...");
        foreach (var file in Directory.EnumerateFiles(dir))
        {
            var add = this.MoveFileToDir(file);
            SafeAdd(add);
        }
        Console.WriteLine("Finished indexing.");
    }
}