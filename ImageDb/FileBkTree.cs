using System.Collections;
using Newtonsoft.Json;

namespace ImageDb;

/// <summary>
/// Data structure which stores file paths. This structure is implemented
/// as a BK-Tree. This allows you to quickly find files which are similar to
/// other files. File comparison is done using the hash function, which should
/// yield a perceptual hash for the file. Hamming distance is used for the
/// distance between two hashes. This tree allows two files to exist with the
/// same hash, but duplicate paths are not allowed.
///
/// The hash function is expected to produce a perceptual hash for the file.
/// A file's hash should be the same every time, and similar files should yield
/// similar hashes.
///
/// For a given file node, each outgoing edge has a distance value. The entire
/// subtree under an edge with distance k will have hashes that differ from the
/// parent node by k. This means if a node has an outgoing edge with a distance
/// of 0, then every node in that subtree has a hash equal to the parent.
/// </summary>
[JsonObject]
public class FileBkTree : IEnumerable<string>
{
    /// <summary>
    /// Function which can generate a perceptual hash for a file.
    /// </summary>
    [JsonIgnore]
    public Func<string, long> HashFunction { get; set; }
    
    /// <summary>
    /// Function that gives the distance between hashes.
    /// </summary>
    [JsonIgnore]
    public Func<long, long, int> HashDistance { get; set; }

    [JsonProperty]
    private FileNode _root;
    /// <summary>
    /// Root element.
    /// </summary>
    [JsonIgnore]
    public string Root => _root?.Path;

    /// <summary>
    /// Add a path to the tree with the associated hash.
    /// </summary>
    /// <param name="path">File path</param>
    /// <param name="hash">File hash</param>
    /// <returns>True if the file was added, false otherwise</returns>
    private bool AddInternal(string path, long hash)
    {
        if (_root == null)
        {
            _root = new FileNode(path, hash);
            return true;
        }

        // Find the parent node to attach to
        var current = _root;
        int diff;
        while (true)
        {
            diff = HashDistance(hash, current.Hash);
            // If the node has an outgoing edge with this difference
            // value, explore it, otherwise create the new node here.
            var child = current.GetSubTree(diff);
            if (child == null) break;
            // Don't create a new entry if it already exists
            if (child.Path == path) return false;
            current = child;
        }
        
        current.AddSubTree(new FileNode(path, hash), diff);
        return true;
    }

    /// <summary>
    /// Add a file path to the tree.
    /// </summary>
    /// <param name="path">File path</param>
    /// <returns>True if the file was added, false otherwise</returns>
    public bool Add(string path)
    {
        return AddInternal(path, HashFunction(path));
    }

    /// <summary>
    /// Remove a file path from the tree. Once the node is found and removed.
    /// All of its descendants will be re-added to the tree.
    /// </summary>
    /// <param name="path">File path</param>
    /// <returns>True if the file path existed and was removed, false otherwise.</returns>
    public bool Remove(string path)
    {
        if (_root == null) return false;
        var (parent, node) = LookupExactInternal(path, HashFunction(path));
        if (node == null) return false;
        
        if (parent != null)
        {
            // If not the root node, delete this node from the parent
            var diff = HashDistance(parent.Hash, node.Hash);
            parent.SubTrees.Remove(diff);
        }
        // Add descendants back to the tree
        var replace = node.GetDetailedSubTreeInfo();
        foreach (var (nodePath, hash) in replace)
        {
            AddInternal(nodePath, hash);
        }
        return true;
    }
    
    /// <summary>
    /// Get all nodes which are within a tolerance of the specified hash.
    /// </summary>
    /// <param name="hash">Test hash</param>
    /// <param name="popTolerance">Amount hashes may differ from the argument</param>
    /// <returns>Sequence of similar nodes</returns>
    private IEnumerable<FileNode> LookupAllInternal(long hash, int popTolerance = 0)
    {
        if (_root == null) yield break;

        var queue = new Queue<FileNode>();
        queue.Enqueue(_root);
        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            var diff = HashDistance(hash, node.Hash);
            if (diff <= popTolerance) yield return node;
            var maxDiff = diff + popTolerance;
            foreach (var (d, subTree) in node.SubTrees)
            {
                if (Math.Abs(d - diff) <= maxDiff) queue.Enqueue(subTree);
            }
        }
    }
    
    /// <summary>
    /// Get a node for the exact file path.
    /// This differs from the regular lookup method where the filename must also
    /// match in order to find the correct node.
    /// </summary>
    /// <param name="path">File path</param>
    /// <param name="hash">File hash</param>
    /// <returns>Parent node and node.</returns>
    private (FileNode Parent, FileNode Node) LookupExactInternal(string path, long hash)
    {
        if (_root == null) return (null, null);

        var queue = new Queue<(FileNode, FileNode)>();
        queue.Enqueue((null, _root));
        var bestDiff = 64L;
        while (queue.Count > 0)
        {
            var (parent, node) = queue.Dequeue();
            if (node.Path == path) return (parent, node);
            var diff = HashDistance(hash, node.Hash);
            if (diff < bestDiff)
            {
                bestDiff = diff;
            }
            foreach (var (d, subTree) in node.SubTrees)
            {
                if (Math.Abs(d - diff) <= bestDiff) queue.Enqueue((node, subTree));
            }
        }

        return (null, null);
    }

    /// <summary>
    /// Get a similar node to the given hash. This will find the first node with
    /// the most similar hash to the argument. If multiple files have the same hash,
    /// any of them may be returned.
    /// </summary>
    /// <param name="hash">File hash</param>
    /// <returns>Parent node and node, or null nodes if the tree is empty.</returns>
    private (FileNode Parent, FileNode Node) LookupInternal(long hash)
    {
        if (_root == null) return (null, null);

        var queue = new Queue<(FileNode, FileNode)>();
        queue.Enqueue((null, _root));
        FileNode nodeParent = null;
        FileNode bestNode = null;
        var bestDiff = 64L;
        while (queue.Count > 0)
        {
            var (parent, node) = queue.Dequeue();
            var diff = HashDistance(hash, node.Hash);
            if (diff < bestDiff)
            {
                nodeParent = parent;
                bestNode = node;
                bestDiff = diff;
            }
            if (bestDiff == 0) break;
            foreach (var (d, subTree) in node.SubTrees)
            {
                if (Math.Abs(d - diff) < bestDiff) queue.Enqueue((node, subTree));
            }
        }

        return (nodeParent, bestNode);
    }

    /// <summary>
    /// Check if this tree contains the given file path.
    /// </summary>
    /// <param name="path">File path</param>
    /// <returns>True if the file path exists in this tree, false otherwise.</returns>
    public bool Contains(string path) => LookupExactInternal(path, HashFunction(path)).Node != null;

    /// <summary>
    /// Get the most similar file in this tree to the given path. If the given
    /// path already exists in the tree, this may return the argument.
    /// </summary>
    /// <param name="path">File path</param>
    /// <returns>Similar file path, or null if the tree is empty.</returns>
    public string Lookup(string path) => Lookup(HashFunction(path));
    
    /// <summary>
    /// This is the same as the Lookup method, but also returns the distance between
    /// the image hash and the hash found in the tree.
    /// </summary>
    /// <param name="path">File path</param>
    /// <returns>Similar file path and hash distance, or null and 0 if the tree is empty.</returns>
    public (string Path, int Difference) LookupDistance(string path) => LookupDistance(HashFunction(path));

    /// <summary>
    /// Get a similar file path to the given hash. If multiple files have the same
    /// hash, any one of them may be returned.
    /// </summary>
    /// <param name="hash">File hash</param>
    /// <returns>Similar file path, or null if the tree is empty.</returns>
    public string Lookup(long hash) => LookupInternal(hash).Node?.Path;
    
    /// <summary>
    /// Get the most similar file to the given hash. If multiple files have the same
    /// hash, any one of them may be returned.
    /// </summary>
    /// <param name="hash">File hash</param>
    /// <returns>File path and hash distance, or null and 0 if the tree is empty.</returns>
    private (string Path, int Difference) LookupDistance(long hash)
    {
        var result = LookupInternal(hash);
        if (result.Node == null) return (null, 0);
        return (result.Node.Path, HashDistance(result.Node.Hash, hash));
    }

    /// <summary>
    /// Get all file paths which are within a given tolerance of the argument.
    /// </summary>
    /// <param name="path">File path</param>
    /// <param name="popTolerance">Tolerance</param>
    /// <returns>Similar file paths</returns>
    public IEnumerable<string> LookupAll(string path, int popTolerance = 0)
    {
        return LookupAll(HashFunction(path), popTolerance);
    }

    /// <summary>
    /// Get all file paths which are withing a given tolerance of the argument.
    /// </summary>
    /// <param name="hash">File hash</param>
    /// <param name="popTolerance">Tolerance</param>
    /// <returns>Similar file paths</returns>
    public IEnumerable<string> LookupAll(long hash, int popTolerance = 0)
    {
        return LookupAllInternal(hash, popTolerance).Select(node => node.Path);
    }
    
    /// <summary>
    /// Get all file paths which are within a given tolerance of the argument.
    /// This also returns the distance between the file and the hash.
    /// </summary>
    /// <param name="hash">File hash</param>
    /// <param name="popTolerance">Tolerance</param>
    /// <returns>Similar file paths and distances</returns>
    public IEnumerable<(string Path, int Distance)> LookupAllDist(long hash, int popTolerance = 0)
    {
        return LookupAllInternal(hash, popTolerance).Select(node => (node.Path, HashDistance(hash, node.Hash)));
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private IEnumerator<string> EmptyEnumerator()
    {
        yield break;
    }

    public IEnumerator<string> GetEnumerator() => _root?.GetEnumerator() ?? EmptyEnumerator();
}

/// <summary>
/// A node of the BK-Tree. This stores the file path and hash and a
/// collection of subtrees.
/// </summary>
[JsonObject]
public class FileNode : IEnumerable<string>
{
    public string Path { get; init; }
    public long Hash { get; init; }

    [JsonProperty]
    private Dictionary<int, FileNode> _subTrees;
    [JsonIgnore]
    internal Dictionary<int, FileNode> SubTrees => _subTrees;

    public FileNode() { }

    public FileNode(string path, long hash)
    {
        Path = path;
        Hash = hash;
        _subTrees = new Dictionary<int, FileNode>();
    }

    // Get the sub tree with the given distance
    internal FileNode GetSubTree(int diff) => _subTrees.GetValueOrDefault(diff);

    // Add a sub tree
    internal void AddSubTree(FileNode tree, int diff) => _subTrees.Add(diff, tree);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<string> GetEnumerator()
    {
        return _subTrees.SelectMany(pair => pair.Value).Prepend(Path).GetEnumerator();
    }

    public IEnumerable<(string Path, long Hash)> GetDetailedSubTreeInfo()
    {
        return _subTrees.SelectMany(pair => pair.Value.GetDetailedSubTreeInfo());
    }

    public override string ToString()
    {
        return $"({Path}, {Hash})";
    }
}