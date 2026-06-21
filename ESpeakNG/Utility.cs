using System;
using System.IO;
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
}
