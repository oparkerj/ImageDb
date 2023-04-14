using ImageDb.Common;
using ImageMagick;

namespace ImageDb.Data;

/// <summary>
/// Helper class that handles loading and saving the database files.
/// </summary>
public class ImageDbFileHandler : IDisposable
{
    /// <summary>
    /// Config to use to load and save files.
    /// </summary>
    public ImageDbConfig Config { get; set; }
    
    private FileBkTree _tree;
    private HashSet<string> _used;

    /// <summary>
    /// Access the main database. Getting this property will
    /// cause the file to be loaded.
    /// </summary>
    public FileBkTree Tree
    {
        get
        {
            LoadTree();
            return _tree;
        }
        set => _tree = value;
    }

    /// <summary>
    /// Set whether or not the tree has been updated. This controls
    /// if the tree will be written to disk.
    /// </summary>
    public bool TreeUpdated { get; set; }

    /// <summary>
    /// Access the usage file. Getting this property will
    /// cause the file to be loaded.
    /// </summary>
    public HashSet<string> Used
    {
        get
        {
            LoadUsage();
            return _used;
        }
        set => _used = value;
    }

    /// <summary>
    /// Set whether or not the usage file has been updated. This controls
    /// if the file will be written to disk.
    /// </summary>
    public bool UsageUpdated { get; set; }

    public void Dispose()
    {
        SaveAll();
    }

    /// <summary>
    /// Save all files from this handler.
    /// </summary>
    public void SaveAll()
    {
        SaveTree();
        SaveUsage();
    }

    /// <summary>
    /// Load the main database.
    /// If the tree is already loaded, this does nothing.
    /// If the tree does not exist, an empty one is loaded.
    /// </summary>
    public void LoadTree()
    {
        if (_tree != null) return;
        TreeUpdated = false;
        
        _tree = Config.FileHandler.LoadTree(Config.DatabaseRelative);
        if (_tree == null)
        {
            TreeUpdated = true;
            _tree = new FileBkTree();
        }
        
        _tree.HashFunction = s => PHash.ComputeHash(Path.Combine(Config.ImageFolderRelative, s));
        _tree.HashDistance = PHash.HashDistance;
    }

    /// <summary>
    /// Unload the tree without saving changes.
    /// </summary>
    public void DropTree()
    {
        _tree = null;
        TreeUpdated = false;
    }

    /// <summary>
    /// Write the tree to disk if it is loaded and TreeUpdated is set to true.
    /// </summary>
    public void SaveTree()
    {
        if (!TreeUpdated || _tree == null) return;
        Config.FileHandler.WriteTree(_tree, Config.DatabaseRelative, Config.ShowJson);
        TreeUpdated = false;
        Config.Update("Saved database.");
    }

    /// <summary>
    /// Load the usage file.
    /// If the file is already loaded, this does nothing.
    /// If the file does not exist, an empty one is loaded.
    /// </summary>
    public void LoadUsage()
    {
        if (_used != null) return;
        UsageUpdated = false;

        _used = Config.FileHandler.LoadUsageFile(Config.UsageFileRelative);
        if (_used == null)
        {
            UsageUpdated = true;
            _used = new HashSet<string>();
        }
    }

    /// <summary>
    /// Unload the usage without saving changes.
    /// </summary>
    public void DropUsage()
    {
        _used = null;
        UsageUpdated = false;
    }

    /// <summary>
    /// Write the usage to disk if it is loaded and UsageUpdated is set to true.
    /// </summary>
    public void SaveUsage()
    {
        if (!UsageUpdated || _used == null) return;
        Config.FileHandler.WriteUsageFile(_used, Config.UsageFileRelative, Config.ShowJson);
        UsageUpdated = false;
        Config.Update("Saved usage file.");
    }

    /// <summary>
    /// Try to add an image to the database.
    /// This method will cause the tree to be loaded.
    /// </summary>
    /// <param name="path">Regular file path</param>
    /// <returns>True if the image was added, false otherwise.</returns>
    public bool TryAddImage(string path)
    {
        if (string.IsNullOrEmpty(path)) return false;
        
        try
        {
            var pathRelative = Config.RelativeToImageFolder(path);
            var added = Tree.Add(pathRelative);
            if (added)
            {
                TreeUpdated = true;
                Config.Update($"Added \"{pathRelative}\"");
            }
            else
            {
                Config.Update($"Skipping existing image \"{pathRelative}\"");
            }
            return added;
        }
        catch (MagickException e)
        {
            Config.Update("Encountered error while trying to add image.");
            Config.Update($"  Image: {path}");
            Config.Update($"  Error: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Remove a file from the tree.
    /// This method will cause the tree to be loaded.
    /// </summary>
    /// <param name="path">Normal file path.</param>
    /// <returns>True if the image was removed, false otherwise.</returns>
    public bool RemoveImage(string path)
    {
        var pathRelative = Config.RelativeToImageFolder(path);
        var removed = Tree.Remove(pathRelative);
        if (removed)
        {
            Config.Update($"Removed file: \"{pathRelative}\"");
            TreeUpdated = true;
        }
        return removed;
    }

    /// <summary>
    /// Add an image to the usage file.
    /// this method will cause the usage file to be loaded.
    /// </summary>
    /// <param name="path">Image path relative to the image folder</param>
    /// <returns>True if the image was added to used, false otherwise.</returns>
    public bool AddUsage(string path)
    {
        var result = Used.Add(path);
        UsageUpdated |= result;
        return result;
    }

    /// <summary>
    /// Remove an image from the usage file.
    /// This method will cause the usage file to be loaded.
    /// </summary>
    /// <param name="path">Image path relative to the image folder.</param>
    /// <returns>True if the image was removed, false otherwise.</returns>
    public bool RemoveUsage(string path)
    {
        var result = Used.Remove(path);
        UsageUpdated |= result;
        return result;
    }
}