using ImageDb.Actions;
using ImageDb.Common;
using ImageDb.Data;

namespace ImageDb.CLI.Actions;

/// <summary>
/// This will first run a lookup on the file to find the most
/// similar image. Given the result, a prompt will appear to add the
/// image. If the answer is yes, the image is added to the database,
/// otherwise nothing happens.
/// </summary>
public class Insert : IAction
{
    public static string Usage => "insert <file>";

    public ImageDbConfig Config { get; set; }
    
    public void Execute(string file)
    {
        using var db = new ImageDbFileHandler {Config = Config};

        Lookup.Execute(file, 0, true, db);
        
        Console.Write("Add file? (y/n): ");
        var response = Console.ReadLine();
        if (response != "y") return;

        AddImage.Execute(file, db);
    }
}