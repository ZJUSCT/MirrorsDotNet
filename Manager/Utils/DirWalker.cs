using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Manager.Models;

namespace Manager.Utils;

/// <summary>
/// Scan Release Directories to generate index
/// </summary>
public class DirWalker
{
    /// <summary>
    /// Traverse the directory tree and generate the index
    /// ref: https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/file-system/how-to-iterate-through-a-directory-tree#examples
    /// </summary>
    /// <param name="indexPath">index root path</param>
    /// <param name="regexPattern">regex to match the filename</param>
    /// <param name="sortBy">string to sort by</param>
    /// <returns>List of UrlItems</returns>
    /// <exception cref="ArgumentException"></exception>
    public static List<MirrorStatus.UrlItem> GenIndex(string indexPath, string regexPattern, string sortBy)
    {
        // Data structure to hold names of subfolders to be
        // examined for files.
        var dirs = new Stack<string>(20);
        var root = $"{Constants.ContentPath}{indexPath}";
        var rx = new Regex(@$"^{regexPattern}$", RegexOptions.Compiled);
        var res = new List<MirrorStatus.UrlItem>();

        if (!Directory.Exists(root))
        {
            throw new ArgumentException();
        }

        dirs.Push(root);

        while (dirs.Count > 0)
        {
            var currentDir = dirs.Pop();
            string[] subDirs;
            try
            {
                subDirs = Directory.GetDirectories(currentDir);
            }
            // An UnauthorizedAccessException exception will be thrown if we do not have
            // discovery permission on a folder or file. It may or may not be acceptable
            // to ignore the exception and continue enumerating the remaining files and
            // folders. It is also possible (but unlikely) that a DirectoryNotFound exception
            // will be raised. This will happen if currentDir has been deleted by
            // another application or thread after our call to Directory.Exists. The
            // choice of which exceptions to catch depends entirely on the specific task
            // you are intending to perform and also on how much you know with certainty
            // about the systems on which this code will run.
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message);
                continue;
            }
            catch (DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
                continue;
            }

            string[] files;
            try
            {
                files = Directory.GetFiles(currentDir);
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message);
                continue;
            }
            catch (DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
                continue;
            }

            // Perform the required action on each file here.
            // Modify this block to perform your required task.
            foreach (var file in files)
            {
                try
                {
                    // Perform whatever action is required in your scenario.
                    var fi = new FileInfo(file);
                    Console.WriteLine("{0}: {1}, {2}", fi.Name, fi.Length, fi.CreationTime);
                    var matches = rx.Matches(fi.Name);
                    if (matches.Count == 0) continue; // file not match
                    res.Add(new MirrorStatus.UrlItem
                    {
                        Name = fi.Name,
                        Url = $"/{Path.GetRelativePath(Constants.ContentPath, fi.FullName)}",
                        SortKey = Regex.Replace(fi.Name, regexPattern, sortBy)
                    });
                }
                catch (FileNotFoundException e)
                {
                    // If file was deleted by a separate application
                    //  or thread since the call to TraverseTree()
                    // then just continue.
                    Console.WriteLine(e.Message);
                }
            }

            // Push the subdirectories onto the stack for traversal.
            // This could also be done before handing the files.
            foreach (var subDir in subDirs)
            {
                if (IsSymbolic(subDir)) continue;
                dirs.Push(subDir);
            }
        }

        return res.OrderBy(o => o.SortKey).ToList();
    }
    
    /// <summary>
    /// Check if dir is symbolic link
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static bool IsSymbolic(string path)
    {
        var pathInfo = new FileInfo(path);
        return pathInfo.Attributes.HasFlag(FileAttributes.ReparsePoint);
    }
}