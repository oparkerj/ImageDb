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

    public void Execute(string file)
    {
        if (!File.Exists(file))
        {
            throw new ArgumentException($"File doesn't exist \"{file}\"");
        }

        file = this.MoveFileToDir(file);
        SafeAdd(file);
    }
    
    public override void Execute() => Execute(GetArg(0));
}