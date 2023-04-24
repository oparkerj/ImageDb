using System.IO.Compression;
using Newtonsoft.Json;

namespace ImageDb.Data;

/// <summary>
/// Provides reading and writing for configuration files.
/// </summary>
public class ConfigFileHandler
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
        using var file = File.Open(path, FileMode.Create);
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
    /// Read a json file and use it to populate an object.
    /// </summary>
    /// <param name="path">File path.</param>
    /// <param name="value">Object to populate.</param>
    /// <returns>True if the object was populated, false otherwise.</returns>
    public bool ReadJson(string path, object value)
    {
        if (!File.Exists(path)) return false;
        using var data = File.OpenRead(path);
        using var reader = new StreamReader(data);
        using var jsonReader = new JsonTextReader(reader);
        _serializer.Populate(jsonReader, value);
        return true;
    }

    /// <summary>
    /// Write the object to a json file.
    /// </summary>
    /// <param name="obj">Object.</param>
    /// <param name="path">File path.</param>
    /// <typeparam name="T">Object type.</typeparam>
    public void WriteJson<T>(T obj, string path)
    {
        using var file = File.Open(path, FileMode.Create);
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
    /// Read the usage file.
    /// </summary>
    /// <param name="path">Path of usage file.</param>
    /// <returns>Usage file data.</returns>
    public HashSet<string> LoadUsageFile(string path)
    {
        return ReadGZipped<HashSet<string>>(path);
    }

    /// <summary>
    /// Write the usage file.
    /// </summary>
    /// <param name="used">Usage data.</param>
    /// <param name="path">Usage file path.</param>
    /// <param name="plain">Corresponds to the includeUncompressed option in WriteGZipped.</param>
    public void WriteUsageFile(HashSet<string> used, string path, bool plain = false)
    {
        WriteGZipped(used, path, plain);
    }
}