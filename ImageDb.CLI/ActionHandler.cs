using ImageDb.Common;

namespace ImageDb.CLI;

public interface IActionHandler
{
    void Execute(ImageDbConfig config, ArgReader args);
}

public class ActionHandler<T> : IActionHandler
    where T : IAction, new()
{
    public string Name { get; }
    public Action<T, ArgReader> Action { get; init; }

    public ActionHandler(string name, Action<T, ArgReader> action)
    {
        Name = name;
        Action = action;
    }

    public void Execute(ImageDbConfig config, ArgReader args)
    {
        var instance = new T {Config = config};
        Action(instance, args);
    }
}