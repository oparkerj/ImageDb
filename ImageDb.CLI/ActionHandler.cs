using ImageDb.Common;

namespace ImageDb.CLI;

public abstract class ActionHandler
{
    public abstract void Execute(ImageDbConfig config, ArgReader args);
}

public class ActionHandler<T> : ActionHandler
    where T : IAction, new()
{
    public string Name { get; }
    public Action<T, ArgReader> Action { get; init; }

    public ActionHandler(string name, Action<T, ArgReader> action)
    {
        Name = name;
        Action = action;
    }

    public override void Execute(ImageDbConfig config, ArgReader args)
    {
        var instance = new T {Config = config};
        Action(instance, args);
    }
}