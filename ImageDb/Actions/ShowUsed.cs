namespace ImageDb.Actions;

/// <summary>
/// Print out the list of files marked as used.
/// </summary>
public class ShowUsed : ActionBase, IActionUsage
{
    public static string Usage => "showUsed";
    
    public ShowUsed(ArgReader args) : base(args) { }
    
    public override void Execute()
    {
        var used = this.LoadUseFile();

        void NoUsed() => Console.WriteLine("No files are used.");
        
        if (used != null)
        {
            var one = false;
            foreach (var s in used)
            {
                one = true;
                Console.WriteLine(s);
            }
            if (!one) NoUsed();
        }
        else NoUsed();
    }
}