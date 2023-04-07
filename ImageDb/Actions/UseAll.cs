namespace ImageDb.Actions;

/// <summary>
/// This is equivalent to running "use [file]" for every
/// file in a directory.
/// </summary>
public class UseAll : ActionBase, IActionUsage
{
    public static string Usage => "useAll <dir>";
    
    public UseAll(ArgReader args) : base(args) { }

    public void Execute(string dir)
    {
        if (!Directory.Exists(dir))
        {
            throw new ArgumentException($"Cannot find directory \"{dir}\"");
        }

        foreach (var file in Directory.EnumerateFiles(dir))
        {
            Console.WriteLine($"Using {file}");
            var options = ArgReader.FromCommand($"use {file}");
            var use = new Use(options);
            use.Execute();
        }
        Console.WriteLine("Finished processing files.");
    }
    
    public override void Execute() => Execute(GetArg(0));
}