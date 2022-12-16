namespace ImageDb.Actions;

/// <summary>
/// This is equivalent to running "use [file]" for every
/// file in a directory.
/// </summary>
public class UseAll : ActionBase, IActionUsage
{
    public static string Usage => "useAll <dir>";
    
    public UseAll(ArgReader args) : base(args) { }
    
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

        foreach (var file in Directory.EnumerateFiles(dir))
        {
            Console.WriteLine($"Using {file}");
            var options = ArgReader.FromCommand($"use {file}");
            var use = new Use(options);
            use.Execute();
        }
        Console.WriteLine("Finished processing files.");
    }
}