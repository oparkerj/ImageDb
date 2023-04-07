using ImageDb.Actions;
using ImageMagick;

namespace ImageDb;

/// <summary>
/// Base class for the actions that the program can execute.
/// </summary>
public abstract class ActionBase
{
    /// <summary>
    /// The arguments provided to the action.
    /// </summary>
    public readonly ArgReader Args;

    private StoredData _data = new();
    /// <summary>
    /// Gives access to reading and writing information stored
    /// in configuration files.
    /// </summary>
    public StoredData Data => _data;

    private FileBkTree _tree;
    /// <summary>
    /// If this is set to true, the tree will be saved to disk
    /// during the Finish method.
    /// </summary>
    protected bool TreeUpdated;
    
    private Config _config;
    /// <summary>
    /// If this is set to true, the config will be saved to disk
    /// during the Finish method.
    /// </summary>
    protected bool ConfigUpdated;
    
    public ActionBase(ArgReader args) => Args = args;

    /// <summary>
    /// Access the config file. Will be loaded from disk if necessary.
    /// </summary>
    public Config Config
    {
        get
        {
            LoadConfig();
            return _config;
        }
    }

    /// <summary>
    /// Access the tree file. Will be loaded from disk is necessary.
    /// </summary>
    public FileBkTree Tree
    {
        get
        {
            LoadTree();
            return _tree;
        }
    }

    /// <summary>
    /// Make the given path relative to the RelativeBase configuration option.
    /// Paths which are rooted will not be changed.
    /// The relative path can be overridden with the --relativeBase option.
    /// </summary>
    /// <param name="path">File path.</param>
    /// <returns>Path relative to configured path.</returns>
    private string UseRelativeBase(string path)
    {
        if (Path.IsPathRooted(path)) return path;
        var relativeBase = Args.GetOption("relativeBase") ?? Config.RelativeBase;
        return Path.Join(relativeBase, path);
    }
    
    /// <summary>
    /// Get the path to the configured database.
    /// This can be overridden with the --database option.
    /// </summary>
    public string DatabasePath => UseRelativeBase(Args.GetOption("database") ?? Config.Database);

    /// <summary>
    /// Get the path to the configured image folder.
    /// This can be overridden with the --folder option.
    /// </summary>
    public string ImageDirPath => UseRelativeBase(Args.GetOption("folder") ?? Config.ImageFolder);

    /// <summary>
    /// Get the path to the configured usage file.
    /// This can be overriden with the --usefile option.
    /// </summary>
    public string UseFile => UseRelativeBase(Args.GetOption("usefile") ?? Config.UsageFile);
    
    /// <summary>
    /// Get the path to the config file.
    /// This is set with the --config option.
    /// Defaults to "config.json"
    /// </summary>
    public string ConfigPath => Args.GetOption("config") ?? "config.json";

    /// <summary>
    /// Load the config file from disk if it is not already loaded.
    /// </summary>
    protected void LoadConfig()
    {
        if (_config != null) return;
        _config = _data.LoadConfig(ConfigPath);
        if (_config == null)
        {
            ConfigUpdated = true;
            _config = new Config();
        }
    }

    /// <summary>
    /// Load the tree file from disk if it is not already loaded.
    /// </summary>
    protected void LoadTree()
    {
        if (_tree != null) return;
        LoadConfig();
        _tree = _data.LoadTree(DatabasePath);
        if (_tree == null)
        {
            TreeUpdated = true;
            _tree = new FileBkTree();
        }
        _tree.HashFunction = s => PHash.ComputeHash(Path.Combine(ImageDirPath, s));
        _tree.HashDistance = PHash.HashDistance;
    }

    /// <summary>
    /// Get the specified number of args after the command name.
    /// If the command is "name arg1 arg2" then getting 1 arg returns [arg1].
    /// </summary>
    /// <param name="argCount">Number of arguments to get.</param>
    /// <param name="args">The retrieved args.</param>
    /// <returns>True if there were enough args present as requested.</returns>
    protected bool GetArgs(int argCount, out string[] args)
    {
        if (Args.Count - 1 < argCount)
        {
            args = default;
            return false;
        }
        args = Args.Args[1..(1 + argCount)];
        return true;
    }

    /// <summary>
    /// Get the arg at the specified index, and parse it according
    /// to the parsable type. Index 0 refers to the argument after
    /// the command name.
    /// </summary>
    /// <param name="index">Argument index to get.</param>
    /// <param name="t">Parsed argument.</param>
    /// <typeparam name="T">Type of argument to get.</typeparam>
    /// <returns>True if the argument was present and could successfully
    /// be parsed.</returns>
    protected bool TryGetArg<T>(int index, out T t)
        where T : IParsable<T>
    {
        if (index + 1 < Args.Count)
        {
            return T.TryParse(Args[index + 1], null, out t);
        }
        t = default;
        return false;
    }

    /// <summary>
    /// This should be called when finished using the action so it can save
    /// any configuration files that were modified.
    /// </summary>
    public void Finish()
    {
        LoadConfig();
        if (TreeUpdated && Tree != null)
        {
            Console.WriteLine("Saving database...");
            _data.WriteTree(Tree, DatabasePath, Args.HasOption(ShowJson.Usage));
        }
        if (ConfigUpdated && Config != null)
        {
            Console.WriteLine("Saving config...");
            _data.SaveConfig(Config, ConfigPath);
        }
    }

    /// <summary>
    /// Execute the action.
    /// </summary>
    public abstract void Execute();

    /// <summary>
    /// Attempt to add the given file to the tree.
    /// If there is an exception while calculating the hash of the image,
    /// an error will be printed. If the file already exists in a tree,
    /// a message will be printed.
    /// </summary>
    /// <param name="image">Image path.</param>
    /// <returns>True if the image was added to the tree, false otherwise.</returns>
    protected bool SafeAdd(string image)
    {
        try
        {
            var added = Tree.Add(TreePath(image));
            if (added)
            {
                Console.WriteLine($"Added \"{image}\"");
                TreeUpdated = true;
            }
            else Console.WriteLine($"Skipping existing image \"{image}\"");
            return added;
        }
        catch (MagickException e)
        {
            Console.WriteLine("Encountered error while trying to add image.");
            Console.WriteLine($"  Image: {image}");
            Console.WriteLine($"  Error: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Get the path to the file which will be stored in the tree.
    /// This modifies the given path to be relative to the configured
    /// image folder.
    /// </summary>
    /// <param name="file">File path.</param>
    /// <returns>Relative path.</returns>
    protected string TreePath(string file)
    {
        return Path.GetRelativePath(ImageDirPath, file);
    }
}