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

    public void Execute(string file)
    {
        if (!File.Exists(file))
        {
            throw new ArgumentException($"File doesn't exist \"{file}\"");
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

    public override void Execute() => Execute(GetArg(0));
}