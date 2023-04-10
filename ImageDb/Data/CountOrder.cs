namespace ImageDb.Data;

/// <summary>
/// Defines a sorting order that orders file names in a more friendly way.
/// </summary>
public class CountOrder : IComparer<string>
{
    /// <summary>
    /// Compare file names in a "friendly" way. If two names have numbers in the
    /// same place, they will be sorted in a human friendly way rather than
    /// comparing the characters directly.
    /// For example, file9 will come before file22.
    /// </summary>
    /// <param name="x">First string</param>
    /// <param name="y">Second string</param>
    /// <returns>-1 if x &lt; y, 0 if x == y, 1 if x &gt; y</returns>
    public int Compare(string x, string y)
    {
        if (x == null && y == null) return 0;
        if (y == null) return -1;
        if (x == null) return 1;
        
        var a = x.AsSpan();
        var b = y.AsSpan();
        var ai = 0;
        var bi = 0;
        while (ai < a.Length && bi < b.Length)
        {
            var ac = a[ai];
            var bc = b[bi];
            if (char.IsDigit(ac) && char.IsDigit(bc))
            {
                while (ai < a.Length - 1 && a[ai] == '0') ai++;
                int aEnd;
                for (aEnd = ai + 1; aEnd < a.Length; aEnd++)
                {
                    if (!char.IsDigit(a[aEnd])) break;
                }
                
                while (bi < b.Length - 1 && b[bi] == '0') bi++;
                int bEnd;
                for (bEnd = bi + 1; bEnd < b.Length; bEnd++)
                {
                    if (!char.IsDigit(b[bEnd])) break;
                }

                if (bEnd - bi != aEnd - ai)
                {
                    return (aEnd - ai) - (bEnd - bi);
                }
                var comp = a[ai..aEnd].CompareTo(b[bi..bEnd], StringComparison.Ordinal);
                if (comp != 0) return comp;
                ai = aEnd - 1;
                bi = bEnd - 1;
            }
            else if (ac.CompareTo(bc) is var comp && comp != 0)
            {
                return comp;
            }
            ai++;
            bi++;
        }
        return a.Length - b.Length;
    }
}