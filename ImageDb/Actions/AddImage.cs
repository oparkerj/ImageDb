namespace ImageDb.Actions;

/// <summary>
/// This action adds an image to the database.
/// The file will be moved into the image folder.
/// This does not check if a similar image already exists.
/// </summary>
public class AddImage : ActionBase, IActionUsage
{
    public static string Usage => "add <file>";
    
    public AddImage(ArgReader args) : base(args) { }
    
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

        file = this.MoveFileToDir(file);
        LoadTree();
        SafeAdd(file);
    }
}