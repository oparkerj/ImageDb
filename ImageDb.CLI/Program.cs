using ImageDb.Actions;
using ImageDb.CLI;
using ImageDb.CLI.Actions;
using ImageDb.Common;

var command = new ArgReader(args);
var actions = BuildActions();
var config = LoadConfig(command);
config.Status += Console.WriteLine;
TryExecute();

// Get all actions the program can execute.
static Dictionary<string, IActionHandler> BuildActions()
{
    var actions = new Dictionary<string, IActionHandler>();

    Add<Init>((action, _) => action.Execute());
    Add<IndexDirectory>((action, reader) => action.Execute(reader[1]));
    Add<AddImage>((action, reader) => action.Execute(reader[1]));
    Add<RemoveImage>((action, reader) => action.Execute(reader[1]));
    Add<Lookup>(((action, reader) => action.Execute(reader[1], reader.Get<int>(2))));
    Add<ShowJson>((action, _) => action.Execute());
    Add<Use>((action, reader) => action.Execute(reader[1]));
    Add<UseAll>((action, reader) => action.Execute(reader[1]));
    Add<RemoveUse>((action, reader) => action.Execute(reader[1]));
    Add<Choose>((action, _) => action.Execute());
    Add<ShowUsed>((action, _) => action.Execute());
    Add<Insert>((action, reader) => action.Execute(reader[1]));
    Add<InsertDir>((action, reader) => action.Execute(reader[1], reader.Get<int>(2), reader.GetOrDefault(3, -1)));
    Add<Backup>((action, reader) => action.Execute(reader[1]));

    return actions;

    void Add<T>(Action<T, ArgReader> action)
        where T : IAction, new()
    {
        var name = T.Usage.IndexOf(' ') is var index && index >= 0 ? T.Usage[..index] : T.Usage;
        actions[name] = new ActionHandler<T>(name, action);
    }
}

void TryExecute(bool allowManage = true)
{
    if (command.Count < 1)
    {
        PrintOptions(allowManage);
        return;
    }
    
    var actionName = command[0];
    var handler = actions.GetValueOrDefault(actionName);

    if (allowManage && actionName == "manage")
    {
        Manage();
        return;
    }
    if (handler is null)
    {
        PrintOptions(allowManage);
        return;
    }

    try
    {
        handler.Execute(config, command);
    }
    catch (Exception e)
    {
        Console.WriteLine($"A problem occurred while executing the command: \"{e.Message}\"");
        if (command.HasOption("errors")) Console.WriteLine($"Stack trace: {e.StackTrace}");
        else Console.WriteLine("Run with the --errors option to print the stack trace.");
    }
}

// Enter manage mode aka interactive mode.
void Manage()
{
    while (true)
    {
        Console.Write("Enter command: ");
        var cmd = Console.ReadLine();
        if (cmd is null or "exit") return;
        command.Args = ArgReader.Split(cmd);
        TryExecute(false);
    }
}

static ImageDbConfig LoadConfig(ArgReader reader)
{
    var config = new ImageDbConfig();

    if (reader.TryGetArgValue("config", out var configPath)) config.ConfigPath = configPath;
    config.LoadConfigFile();

    if (reader.TryGetArgValue("database", out var databasePath)) config.Database = databasePath;
    if (reader.TryGetArgValue("folder", out var folderPath)) config.ImageFolder = folderPath;
    if (reader.TryGetArgValue("usefile", out var usagePath)) config.UsageFile = usagePath;
    if (reader.TryGetArgValue("relativeBase", out var relativePath)) config.RelativeBase = relativePath;
    if (reader.HasOption("showjson")) config.ShowJson = true;

    return config;
}

// Print out the usages for the available actions.
void PrintOptions(bool allowManage)
{
    Console.WriteLine($"Options:{Environment.NewLine}" +
                      $"{string.Join(Environment.NewLine, actions.Keys)}{Environment.NewLine}" +
                      $"{(allowManage ? "manage" : "exit")}");
}