namespace ImageDb;

/// <summary>
/// Configuration values to use when running the program.
/// </summary>
public class Config
{
    /// <summary>
    /// File containing the image tree.
    /// </summary>
    public string Database { get; set; } = "images.dat";
    
    /// <summary>
    /// File containing set of images marked as used.
    /// </summary>
    public string UsageFile { get; set; } = "used.dat";
    
    /// <summary>
    /// Path to folder containing images.
    /// </summary>
    public string ImageFolder { get; set; } = "images";
    
    /// <summary>
    /// Format of file names in the image folder.
    /// </summary>
    public string NameFormat { get; set; } = "image{num}{ext}";

    /// <summary>
    /// Base folder for relative paths. Null for current directory.
    /// </summary>
    public string RelativeBase { get; set; } = null;
}