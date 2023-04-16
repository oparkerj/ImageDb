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
public class InsertDir : IAction
{
    public static string Usage => "insertDir <dir> <autoAcceptTolerance> [autoDenyTolerance]";

    public ImageDbConfig Config { get; set; }

    public void Execute(string dir, int tolerance, int autoDeny = -1)
    {
        if (autoDeny >= tolerance)
        {
            throw new ArgumentException("Auto-deny must be less than tolerance.");
        }
        if (!Directory.Exists(dir))
        {
            throw new ArgumentException("Directory doesn't exist.");
        }

        using var db = new ImageDbFileHandler {Config = Config};

        foreach (var file in Directory.GetFiles(dir).Order(new CountOrder()))
        {
            Console.WriteLine($"Checking: {file}");
            
            var (path, difference) = db.Tree.LookupDistance(db.Config.RelativeToImageFolder(file));
            
            if (difference <= autoDeny)
            {
                Console.WriteLine($"Distance: {difference}");
                Console.WriteLine("Skipping");
            }
            else if (difference >= tolerance)
            {
                Console.WriteLine($"Distance: {difference}");
                AddImage.Execute(file, db);
            }
            else
            {
                Console.WriteLine($"Closest distance: {difference}, {path}");
                Console.Write("Add file? (y/n): ");
                var response = Console.ReadLine();
                if (response != "y") continue;
                AddImage.Execute(file, db);
            }
        }
    }
}