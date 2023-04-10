namespace ImageDb.Actions;

/// <summary>
/// Write plain and neatly indented versions of the config files.
/// </summary>
public class ShowJson : ActionBase, IActionUsage
{
    public static string Usage => "showjson";
    
    public ShowJson(ArgReader args) : base(args) { }
    
    public override void Execute()
    {
        LoadTree();
        Args.InsertOption(Usage);
        TreeUpdated = true;

        var used = this.LoadUseFile();
        used ??= new HashSet<string>();
        this.WriteUseFile(used);
    }
}