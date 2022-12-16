namespace ImageDb.Actions;

/// <summary>
/// Create default configuration files.
/// This will create a config file, an empty database, an
/// empty "used" file, and create the default image folder.
/// </summary>
public class Init : ActionBase, IActionUsage
{
    public static string Usage => "init";
    
    public Init(ArgReader args) : base(args) { }
    
    public override void Execute()
    {
        LoadConfig();
        LoadTree();
        TreeUpdated = true;
        ConfigUpdated = true;

        var used = this.LoadUseFile();
        used ??= new HashSet<string>();
        this.WriteUseFile(used);
        var dir = this.GetFolderOption();
        Directory.CreateDirectory(dir);
    }
}