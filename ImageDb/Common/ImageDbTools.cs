namespace ImageDb.Common;

public static class ImageDbTools
{
    /// <summary>
    /// Get the path to the next file for the given directory.
    /// This will replace "{num}" in the format string with some
    /// number in order to make sure the filename with the given format
    /// can be placed in the folder. The value of the next number is not
    /// guaranteed to be a specific value.
    /// </summary>
    /// <param name="dir">Directory to search.</param>
    /// <param name="ext">File extension including "."</param>
    /// <param name="format">File name format string.</param>
    /// <returns>Path to next file, or null if one could not be found.</returns>
    public static string NextFilePath(string dir, string ext, string format)
    {
        string GetFileName(int num) => format.Replace("{num}", num.ToString());

        var fileNames = Directory.GetFiles(dir)
            .Select(Path.GetFileNameWithoutExtension)
            .ToHashSet();

        var name = Enumerable.Range(1, fileNames.Count)
            .Select(GetFileName)
            .Prepend(GetFileName(fileNames.Count + 1))
            .FirstOrDefault(name => !fileNames.Contains(name));

        return Path.Combine(dir, Path.ChangeExtension(name!, ext));
    }

    /// <summary>
    /// Get the path to the next file to be created in the configured image folder.
    /// "{ext}" in the name format will be replaced with the extension.
    /// "{num}" in the name format will be replaced with a unique number.
    /// The number is not guaranteed to be sequential.
    /// </summary>
    /// <param name="ext">Extension including the dot.</param>
    /// <param name="config">Configuration to use to get image folder path and name format.</param>
    /// <returns>Path to available file.</returns>
    public static string NextFile(string ext, ImageDbConfig config)
    {
        return NextFilePath(config.ImageFolderRelative, ext, config.NameFormat);
    }

    /// <summary>
    /// Move the given file to the configured image folder.
    /// The file will be renamed according to the configured file name format.
    /// The text "{ext}" in the file format will be replaced with the file extension
    /// of the first parameter.
    /// <seealso cref="NextFilePath"/>
    /// </summary>
    /// <param name="file">File path.</param>
    /// <param name="config">Configuration to use to get the image folder path
    /// and the file name format.</param>
    /// <returns>Path to the moved and renamed file.</returns>
    public static string MoveFileToImageDir(string file, ImageDbConfig config)
    {
        var ext = Path.GetExtension(file);
        var dest = NextFile(ext, config);
        File.Move(file, dest);
        config.Update($"Moved file to \"{DirectoryString(dest)}\"");
        return dest;
    }

    /// <summary>
    /// Will return a relative path if the path is within the current working directory.
    /// Otherwise returns the absolute path.
    /// </summary>
    /// <param name="path">File path</param>
    /// <returns>Input path</returns>
    public static string DirectoryString(string path)
    {
        var cwd = Path.GetFullPath(Environment.CurrentDirectory);
        var fullPath = Path.GetFullPath(path);
        return fullPath.StartsWith(cwd) ? Path.GetRelativePath(cwd, fullPath) : fullPath;
    }

    /// <summary>
    /// Transform a sequence of paths by prepending a directory path.
    /// </summary>
    /// <param name="list">Relative paths</param>
    /// <param name="dir">Base path</param>
    /// <returns>Transformed paths</returns>
    public static IEnumerable<string> FromRelativePaths(this IEnumerable<string> list, string dir)
    {
        return list.Select(relative => Path.Join(dir, relative));
    }
}