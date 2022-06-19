using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Manager.Models;
using Manager.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Manager.Services;

/// <summary>
/// Scan Release Directories to generate index
/// </summary>
public class IndexService : IIndexService
{
    private readonly ILogger<IndexService> _logger;
    private readonly MirrorContext _context;
    public IndexService(MirrorContext context, ILogger<IndexService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Traverse the directory tree and generate the index
    /// ref: https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/file-system/how-to-iterate-through-a-directory-tree#examples
    /// </summary>
    /// <param name="indexPath">index root path</param>
    /// <param name="regexPattern">regex to match the filename</param>
    /// <param name="sortBy">string to sort by</param>
    /// <param name="excludePattern">exclude path from index</param>
    /// <returns>List of UrlItems</returns>
    /// <exception cref="ArgumentException"></exception>
    public List<UrlItem> GenIndex(string indexPath, string regexPattern, string sortBy,
        string excludePattern = null)
    {
        // Data structure to hold names of subfolders to be
        // examined for files.
        var dirs = new Stack<string>(20);
        var root = $"{Constants.ContentPath}{indexPath}";
        var rx = new Regex(@$"^{regexPattern}$", RegexOptions.Compiled);
        var excludeRx = new Regex(@$"{excludePattern}", RegexOptions.Compiled);
        var res = new List<UrlItem>();

        if (!Directory.Exists(root))
        {
            _logger.LogWarning("Root dir {Root} not found!", root);
            return res;
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
                _logger.LogWarning(e.Message);
                continue;
            }
            catch (DirectoryNotFoundException e)
            {
                _logger.LogWarning(e.Message);
                continue;
            }

            string[] files;
            try
            {
                files = Directory.GetFiles(currentDir);
            }
            catch (UnauthorizedAccessException e)
            {
                _logger.LogWarning(e.Message);
                continue;
            }
            catch (DirectoryNotFoundException e)
            {
                _logger.LogWarning(e.Message);
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
                    var matches = rx.Matches(fi.Name);
                    if (matches.Count == 0) continue; // file not match
                    if (excludeRx.IsMatch(fi.FullName)) continue; // file path is excluded
                    _logger.LogInformation("{0}: {1}, {2}", fi.Name, fi.Length, fi.CreationTime);
                    res.Add(new UrlItem
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
                    _logger.LogWarning(e.Message);
                }
            }

            // Push the subdirectories onto the stack for traversal.
            // This could also be done before handing the files.
            foreach (var subDir in subDirs)
            {
                if (IsSymbolic(subDir)) continue;
                if (excludePattern != null && excludeRx.IsMatch(subDir)) continue;
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

    public async Task GenIndexAsync(string indexId)
    {
        _logger.LogInformation("Update Index: {Id}", indexId);

        var indexConfig = await _context.IndexConfigs.FindAsync(indexId);
        if (indexConfig == null)
        {
            _logger.LogError("Index config {Id} not found", indexId);
            return;
        }

        var registerTargetId = indexConfig.RegisterId;
        var targetMirrorItem = await _context.Mirrors.Include(mirror => mirror.Files).FirstOrDefaultAsync(i => i.Id == registerTargetId);

        if (targetMirrorItem == null)
        {
            _logger.LogError("Target mirror {Id} not found", registerTargetId);
            return;
        }

        var newUrlItems = GenIndex(indexConfig.IndexPath, indexConfig.Pattern, indexConfig.SortBy, indexConfig.ExcludePattern);

        // update url items
        // ref: https://docs.microsoft.com/en-us/ef/core/saving/disconnected-entities#handling-deletes
        foreach (var urlItem in newUrlItems)
        {
            var existingUrlItem = targetMirrorItem.Files.FirstOrDefault(url => url.Url.Equals(urlItem.Url));

            if (existingUrlItem == null)
            {
                targetMirrorItem.Files.Add(urlItem);
            }
            else
            {
                _context.Entry(existingUrlItem).CurrentValues.SetValues(urlItem);
            }
        }

        foreach (var urlItem in targetMirrorItem.Files.Where(urlItem => !newUrlItems.Any(url => url.Url.Equals(urlItem.Url))))
        {
            _context.Remove(urlItem);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Generated index {IndexId}", indexId);
    }
}