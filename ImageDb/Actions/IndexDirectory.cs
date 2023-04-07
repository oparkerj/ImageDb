namespace ImageDb.Actions;

/// <summary>
/// This is equivalent to running "add [file]" on every file
/// in a directory.
/// </summary>
public class IndexDirectory : ActionBase, IActionUsage
{
    public static string Usage => "index <directory>";
    
    public IndexDirectory(ArgReader args) : base(args) { }

    public void Execute(string dir)
    {
        if (!Directory.Exists(dir))
        {
            throw new ArgumentException($"Cannot find directory \"{dir}\"");
        }
        
        Console.WriteLine("Indexing directory...");
        foreach (var file in Directory.EnumerateFiles(dir))
        {
            var add = this.MoveFileToDir(file);
            SafeAdd(add);
        }
        Console.WriteLine("Finished indexing.");
    }
    
    public override void Execute() => Execute(GetArg(0));
}