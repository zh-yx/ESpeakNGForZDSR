using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace ESpeakNG;

internal static class Utility
{
    [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool PathFindOnPath([In, Out] char[] pszFile, string[]? ppszOtherDirs);

    public static string? FindOnPath(string fileName, string[]? additionalDirectories = null)
    {
        const int MAX_PATH = 260;
        if (fileName.Length > MAX_PATH) throw new PathTooLongException();
        char[] buffer = new char[MAX_PATH];
        fileName.CopyTo(0, buffer, 0, fileName.Length);
        return PathFindOnPath(buffer, additionalDirectories) ? new string(buffer, 0, Array.IndexOf(buffer, '\0')) : null;
    }

    public static void EnsurePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;

        path = path.Trim();
        string oldPath = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        string[] paths = oldPath.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (paths.Any(p => p.Equals(path, StringComparison.OrdinalIgnoreCase))) return;

        string newPath = oldPath + Path.PathSeparator + path;
        Environment.SetEnvironmentVariable("PATH", newPath);
    }
}
