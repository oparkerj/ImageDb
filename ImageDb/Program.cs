using ImageDb;
using ImageDb.Actions;

RunAction(args);

// Get all actions the program can execute.
static IEnumerable<(string Usage, Type Type)> AllActions()
{
    static (string, Type) Add<T>()
        where T : ActionBase, IActionUsage
    {
        return (T.Usage, typeof(T));
    }

    yield return Add<Init>();
    yield return Add<IndexDirectory>();
    yield return Add<AddImage>();
    yield return Add<RemoveImage>();
    yield return Add<Lookup>();
    yield return Add<ShowJson>();
    yield return Add<Use>();
    yield return Add<UseAll>();
    yield return Add<RemoveUse>();
    yield return Add<Insert>();
    yield return Add<InsertDir>();
    yield return Add<Choose>();
    yield return Add<ShowUsed>();
}

// Run the action with the given arguments.
static void RunAction(string[] args, bool allowManage = true)
{
    var options = new ArgReader(args);
    if (options.Count < 1)
    {
        PrintOptions(allowManage);
        return;
    }
    var actionName = options[0];

    var (_, type) = AllActions().FirstOrDefault(action => action.Usage.IsCommand(actionName));

    if (allowManage && actionName == "manage")
    {
        Manage();
        return;
    }
    if (type == null)
    {
        PrintOptions(allowManage);
        return;
    }

    var action = (ActionBase) Activator.CreateInstance(type, options)!;
    action.Execute();
    action.Finish();
}

// Enter manage mode aka interactive mode.
static void Manage()
{
    while (true)
    {
        Console.Write("Enter command: ");
        var cmd = Console.ReadLine();
        if (cmd == "exit") return;
        RunAction(ArgReader.Split(cmd), false);
    }
}

// Print out the usages for the available actions.
static void PrintOptions(bool allowManage = true)
{
    Console.WriteLine($"Options:{Environment.NewLine}{
    string.Join(Environment.NewLine, AllActions().Select(action => action.Usage))
    }{Environment.NewLine}{(allowManage ? "manage" : "exit")}");
}