namespace ImageDb.Actions;

/// <summary>
/// This will first run a lookup on the file to find the most
/// similar image. Given the result, a prompt will appear to add the
/// image. If the answer is yes, the image is added to the database,
/// otherwise nothing happens.
/// </summary>
public class Insert : ActionBase, IActionUsage
{
    public static string Usage => "insert <file>";
    
    public Insert(ArgReader args) : base(args) { }

    public void Execute(string file)
    {
        if (!File.Exists(file))
        {
            throw new ArgumentException($"File doesn't exist \"{file}\"");
        }
        
        new Lookup(null).Execute(file);
        Console.Write("Add file? (y/n): ");
        var response = Console.ReadLine();
        if (response != "y") return;
        
        new AddImage(null).Execute(file);
    }
    
    public override void Execute() => Execute(GetArg(0));
}