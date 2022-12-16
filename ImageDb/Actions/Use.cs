namespace ImageDb.Actions;

/// <summary>
/// Manually mark an image as used. If the image does not exist in the database,
/// it will first be added, which will cause the file to be moved to the image
/// folder.
/// </summary>
public class Use : ActionBase, IActionUsage
{
    public static string Usage => "use <file>";
    
    public Use(ArgReader args) : base(args) { }

    public static string NextFileName(string dir, string format)
    {
        var num = Directory.GetFiles(dir).Length;
        return format.Replace("{num}", num.ToString());
    }

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
        if (!Tree.Contains(TreePath(file)))
        {
            Console.WriteLine("Image is not indexed.");
            file = this.MoveFileToDir(file);
            SafeAdd(file);
        }

        var used = this.LoadUseFile();
        used ??= new HashSet<string>();
        if (used.Add(TreePath(file)))
        {
            this.WriteUseFile(used);
            Console.WriteLine("Marked file as used.");
        }
        else
        {
            Console.WriteLine("File has already been used.");
        }
    }
}