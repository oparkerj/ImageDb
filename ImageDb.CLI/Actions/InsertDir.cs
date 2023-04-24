using ImageDb.Actions;
using ImageDb.Common;
using ImageDb.Data;

namespace ImageDb.CLI.Actions;

/// <summary>
/// For each file in a directory:
/// This will first run a lookup on the file to find the most
/// similar image. Given the result, the file may be added or skipped
/// automatically, otherwise a prompt will appear to add the
/// image. If the answer is yes, the image is added to the database,
/// otherwise nothing happens.
/// </summary>
public class InsertDir : IAction
{
    public static string Usage => "insertDir <dir> <autoAcceptTolerance> [autoDenyTolerance]";

    public ImageDbConfig Config { get; set; }

    public void Execute(string dir, int tolerance, int autoDeny = -1)
    {
        using var db = new ImageDbFileHandler {Config = Config};
        foreach (var result in InsertDirectory.Execute(dir, tolerance, db, autoDeny))
        {
            if (result.Inserted == null)
            {
                Console.Write("Add file? (y/n): ");
                var response = Console.ReadLine();
                if (response != "y") continue;
                result.Accept(db);
            }
        }
    }
}