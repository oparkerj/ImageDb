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
        
        new Lookup(ArgReader.FromCommand($"lookup {file}")).Execute();
        Console.Write("Add file? (y/n): ");
        var response = Console.ReadLine();
        if (response != "y") return;
        
        new AddImage(ArgReader.FromCommand($"add {file}")).Execute();
    }
}