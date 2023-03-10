using System.IO.Compression;
using Newtonsoft.Json;

namespace ImageDb;

/// <summary>
/// Provides reading and writing for configuration files.
/// </summary>
public class StoredData
{
    private readonly JsonSerializer _serializer = new();

    /// <summary>
    /// Read a json file with GZip compression.
    /// </summary>
    /// <param name="path">File path.</param>
    /// <typeparam name="T">Json parse type.</typeparam>
    /// <returns>Parsed object.</returns>
    public T ReadGZipped<T>(string path)
    {
        if (!File.Exists(path)) return default;
        using var data = File.OpenRead(path);
        using var gzip = new GZipStream(data, CompressionMode.Decompress);
        using var decompressed = new StreamReader(gzip);
        using var jsonReader = new JsonTextReader(decompressed);
        return _serializer.Deserialize<T>(jsonReader);
    }

    /// <summary>
    /// Write the object to a file with GZip compression.
    /// The object is first serialized to json, and then written to the file.
    /// </summary>
    /// <param name="obj">Object to write.</param>
    /// <param name="path">File path.</param>
    /// <param name="includeUncompressed">If this is true, the raw json will also
    /// be written to disk.</param>
    /// <typeparam name="T">Object type.</typeparam>
    public void WriteGZipped<T>(T obj, string path, bool includeUncompressed = false)
    {
        using var file = File.OpenWrite(path);
        using var gzip = new GZipStream(file, CompressionLevel.Optimal);
        using var writer = new StreamWriter(gzip);
        _serializer.Formatting = Formatting.None;
        _serializer.Serialize(writer, obj);
        if (includeUncompressed)
        {
            WriteJson(obj, path + ".json");
        }
    }

    /// <summary>
    /// Read a standard json file.
    /// </summary>
    /// <param name="path">File path.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>Deserialized json object, or null if the file doesn't exist.</returns>
    public T ReadJson<T>(string path)
    {
        if (!File.Exists(path)) return default;
        using var data = File.OpenRead(path);
        using var reader = new StreamReader(data);
        using var jsonReader = new JsonTextReader(reader);
        return _serializer.Deserialize<T>(jsonReader);
    }

    /// <summary>
    /// Write the object to a json file.
    /// </summary>
    /// <param name="obj">Object.</param>
    /// <param name="path">File path.</param>
    /// <typeparam name="T">Object type.</typeparam>
    public void WriteJson<T>(T obj, string path)
    {
        using var file = File.OpenWrite(path);
        using var writer = new StreamWriter(file);
        _serializer.Formatting = Formatting.Indented;
        _serializer.Serialize(writer, obj);
    }

    /// <summary>
    /// Read the tree file.
    /// </summary>
    /// <param name="path">File path.</param>
    /// <returns>File tree, or null if it doesn't exist.</returns>
    public FileBkTree LoadTree(string path)
    {
        return ReadGZipped<FileBkTree>(path);
    }

    /// <summary>
    /// Write the tree file to disk.
    /// </summary>
    /// <param name="tree">File tree.</param>
    /// <param name="path">File path.</param>
    /// <param name="plain">Corresponds to the includeUncompressed option in WriteGZipped.</param>
    public void WriteTree(FileBkTree tree, string path, bool plain = false)
    {
        WriteGZipped(tree, path, plain);
    }

    /// <summary>
    /// Load the configuration file.
    /// </summary>
    /// <param name="path">File path.</param>
    /// <returns>Config object, or null if the config file does not exist.</returns>
    public Config LoadConfig(string path)
    {
        path ??= "config.json";
        return ReadJson<Config>(path);
    }

    /// <summary>
    /// Write the configuration file.
    /// </summary>
    /// <param name="config">Config object.</param>
    /// <param name="path">File path.</param>
    public void SaveConfig(Config config, string path)
    {
        path ??= "config.json";
        WriteJson(config, path);
    }
}