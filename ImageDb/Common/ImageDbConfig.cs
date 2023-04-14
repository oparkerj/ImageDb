using ImageDb.Data;
using Newtonsoft.Json;

namespace ImageDb.Common;

/// <summary>
/// Configuration values to use when running the program.
/// </summary>
public class ImageDbConfig
{
    /// <summary>
    /// Handler for reading and writing config files.
    /// </summary>
    [JsonIgnore]
    public readonly ConfigFileHandler FileHandler = new();

    /// <summary>
    /// Path to the config file.
    /// </summary>
    [JsonIgnore]
    public string ConfigPath { get; set; } = "config.json";

    /// <summary>
    /// File containing the image tree.
    /// </summary>
    public string Database { get; set; } = "images.dat";

    /// <summary>
    /// Get the database path using the relative base.
    /// </summary>
    [JsonIgnore]
    public string DatabaseRelative => GetRelativePath(Database);

    /// <summary>
    /// File containing set of images marked as used.
    /// </summary>
    public string UsageFile { get; set; } = "used.dat";

    /// <summary>
    /// Get the usage file using the relative base.
    /// </summary>
    [JsonIgnore]
    public string UsageFileRelative => GetRelativePath(UsageFile);

    /// <summary>
    /// Path to folder containing images.
    /// </summary>
    public string ImageFolder { get; set; } = "images";

    /// <summary>
    /// Get the image folder using the relative base.
    /// </summary>
    [JsonIgnore]
    public string ImageFolderRelative => GetRelativePath(ImageFolder);

    /// <summary>
    /// Format of file names in the image folder.
    /// </summary>
    public string NameFormat { get; set; } = "image{num}{ext}";

    /// <summary>
    /// Base folder for relative paths. Null for current directory.
    /// </summary>
    public string RelativeBase { get; set; } = null;

    /// <summary>
    /// Indicates that uncompressed JSON should be written alongside the
    /// compressed data.
    /// </summary>
    [JsonIgnore]
    public bool ShowJson { get; set; } = false;
    
    /// <summary>
    /// Event where status updates are sent while executing actions.
    /// </summary>
    public event Action<string> Status;

    /// <summary>
    /// Send a status update.
    /// </summary>
    /// <param name="status"></param>
    public void Update(string status) => Status?.Invoke(status);

    /// <summary>
    /// Make a path relative to the RelativeBase setting.
    /// </summary>
    /// <param name="path">File path</param>
    /// <returns>Path relative to RelativeBase</returns>
    public string GetRelativePath(string path) => Path.Combine(RelativeBase ?? "", path);

    /// <summary>
    /// Convert a path to be relative to the configured image folder.
    /// </summary>
    /// <param name="path">File path</param>
    /// <returns>Path relative to image folder</returns>
    public string RelativeToImageFolder(string path) => Path.GetRelativePath(ImageFolderRelative, path);

    /// <summary>
    /// Load a config file into the current instance.
    /// </summary>
    /// <returns>True if the file loaded, false otherwise.</returns>
    public bool LoadConfigFile() => FileHandler.ReadJson(ConfigPath, this);

    /// <summary>
    /// Write this config to the config file.
    /// </summary>
    public void SaveConfigFile() => FileHandler.WriteJson(this, ConfigPath);
}