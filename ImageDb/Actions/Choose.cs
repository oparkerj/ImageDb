namespace ImageDb.Actions;

/// <summary>
/// Select an image from the database to mark as "used".
/// Images marked as used will not be chosen again.
/// </summary>
public class Choose : ActionBase, IActionUsage
{
    public static string Usage => "choose";
    
    public Choose(ArgReader args) : base(args) { }
    
    public override void Execute()
    {
        var used = this.LoadUseFile();
        used ??= new HashSet<string>();
        var chosen = Tree.Except(used).FirstOrDefault();
        if (chosen == null)
        {
            Console.WriteLine("There are no unused images.");
            return;
        }
        used.Add(chosen);
        Console.WriteLine("Updating use file...");
        this.WriteUseFile(used);
        Console.WriteLine("Chosen image:");
        Console.WriteLine(chosen);
    }
}