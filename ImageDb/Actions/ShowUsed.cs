using ImageDb.Common;
using ImageDb.Data;

namespace ImageDb.Actions;

/// <summary>
/// Print out the list of files marked as used.
/// </summary>
public class ShowUsed : ActionBase, IActionUsage
{
    public static string Usage => "showUsed";
    
    public ShowUsed(ArgReader args) : base(args) { }

    public List<string> GetRelative()
    {
        return Get().RelativeFromImageDir(ImageDirPath);
    }

    public List<string> Get()
    {
        var used = this.LoadUseFile();
        var result = new List<string>(used?.Count ?? 0);
        if (used != null)
        {
            result.AddRange(used.Order(new CountOrder()));
        }
        return result;
    }
    
    public override void Execute()
    {
        var used = Get();
        if (used.Count == 0)
        {
            Console.WriteLine("No files are used.");
        }
        else
        {
            foreach (var s in used)
            {
                Console.WriteLine(s);
            }
        }
    }
}