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

    // Doesn't really make sense to call this one externally since it
    // is based around asking for console confirmation before adding.
    public bool Execute(string file)
    {
        if (!File.Exists(file))
        {
            throw new ArgumentException($"File doesn't exist \"{file}\"");
        }

        var options = Args.ExtractOptions();
        
        new Lookup(options).Execute(file);
        Console.Write("Add file? (y/n): ");
        var response = Console.ReadLine();
        if (response != "y") return false;
        
        new AddImage(options).Execute(file);
        return true;
    }
    
    public override void Execute() => Execute(GetArg(0));
}