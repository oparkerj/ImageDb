namespace ImageDb.Actions;

/// <summary>
/// This will first run a lookup on the file to find the most
/// similar image. Given the result, a prompt will appear to add the
/// image. If the answer is yes, the image is added to the database,
/// otherwise nothing happens.
/// </summary>
public class InsertDir : ActionBase, IActionUsage
{
    public static string Usage => "insertDir <dir> <autoAcceptTolerance>";
    
    public InsertDir(ArgReader args) : base(args) { }
    
    public override void Execute()
    {
        if (!GetArgs(2, out var args))
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

        if (!int.TryParse(args[1], out var tolerance) || tolerance < 0)
        {
            Console.WriteLine("Invalid tolerance level.");
            return;
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
            if (difference >= tolerance)
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
}