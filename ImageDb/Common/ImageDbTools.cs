﻿namespace ImageDb.Common;

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
    /// <param name="format">File name format string.</param>
    /// <returns>Path to next file, or null if one could not be found.</returns>
    public static string NextFile(string dir, string format)
    {
        string GetFileName(int num) => format.Replace("{num}", num.ToString());
        
        var num = Directory.GetFiles(dir).Length;
        return Enumerable.Range(1, num)
            .Select(GetFileName)
            .Prepend(GetFileName(num + 1))
            .Select(s => Path.Combine(dir, s))
            .FirstOrDefault(s => !File.Exists(s));
    }

    /// <summary>
    /// Move the given file to the configured image folder.
    /// The file will be renamed according to the configured file name format.
    /// The text "{ext}" in the file format will be replaced with the file extension
    /// of the first parameter.
    /// <seealso cref="NextFile"/>
    /// </summary>
    /// <param name="file">File path.</param>
    /// <param name="config">Configuration to use to get the image folder path
    /// and the file name format.</param>
    /// <returns>Path to the moved and renamed file.</returns>
    public static string MoveFileToImageDir(string file, ImageDbConfig config)
    {
        var dir = config.ImageFolderRelative;
        var ext = Path.GetExtension(file);
        var name = config.NameFormat.Replace("{ext}", ext);
        var dest = NextFile(dir, name);
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