namespace ImageDb.Actions;

/// <summary>
/// This will first run a lookup on the file to find the most
/// similar image. Given the result, a prompt will appear to add the
/// image. If the answer is yes, the image is added to the database,
/// otherwise nothing happens.
/// </summary>
public class InsertDir : ActionBase, IActionUsage
{
    public static string Usage => "insertDir <dir> <autoAcceptTolerance> [autoDenyTolerance]";
    
    public InsertDir(ArgReader args) : base(args) { }

    public void Execute(string dir, int tolerance, int autoDeny = -1)
    {
        if (autoDeny >= tolerance)
        {
            throw new ArgumentException("Auto-deny must be less than tolerance.");
        }
        
        void Add(string file)
        {
            var moved = this.MoveFileToDir(file);
            SafeAdd(moved);
        }
        
        foreach (var file in Directory.GetFiles(dir).Order(new CountOrder()))
        {
            Console.WriteLine($"Checking: {file}");
            var (path, difference) = Tree.LookupDistance(TreePath(file));
            if (difference <= autoDeny)
            {
                Console.WriteLine($"Distance: {difference}");
                Console.WriteLine("Skipping");
            }
            else if (difference >= tolerance)
            {
                Console.WriteLine($"Distance: {difference}");
                Add(file);
            }
            else
            {
                Console.WriteLine($"Closest distance: {difference}, {path}");
                Console.Write("Add file? (y/n): ");
                var response = Console.ReadLine();
                if (response != "y") continue;
                Add(file);
            }
        }
    }
    
    public override void Execute()
    {
        if (GetArg(0) is var dir && !Directory.Exists(dir))
        {
            throw new Exception($"Cannot find directory \"{dir}\"");
        }
        if (GetArg<int>(1) is var tolerance && tolerance < 0)
        {
            throw new Exception("Invalid tolerance level.");
        }
        var autoDeny = GetArgOrDefault(2, -1);
        
        Execute(dir, tolerance, autoDeny);
    }
}