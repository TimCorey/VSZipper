using System.IO.Compression;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace ZipperLibrary;

public class Zipper
{
    private List<string> exclusions = new();

    public Zipper(IConfiguration config)
    {
        IConfigurationSection? exclusionSection = config.GetSection("Exclusions");
        string[]? excludedPathsAndExtensions = exclusionSection.Get<string[]>();

        foreach (string excludedPathOrExtension in excludedPathsAndExtensions)
        {
            exclusions.Add(excludedPathOrExtension);
        }
    }

    public void Zip()
    {
        string folder = AppDomain.CurrentDomain.BaseDirectory;
        var slnFile = new DirectoryInfo(folder).GetFiles("*.sln");
        string zipFile = (slnFile?.FirstOrDefault()?.Name[..^4] ?? "Unknown") + ".zip";
        Zip(folder, zipFile);
    }

    public void Zip(string folder, string zipFile)
    {
        try
        {
            using ZipArchive archive = ZipFile.Open(zipFile, ZipArchiveMode.Create);

            var files = new DirectoryInfo(folder)
                .GetFiles("*", SearchOption.AllDirectories)
                .Where(x => (x.Attributes & FileAttributes.Hidden) == 0 && x.FullName != zipFile);

            foreach (var f in files)
            {
                if (exclusions.Count(x => f.FullName.Contains(x)) == 0)
                {
                    archive.CreateEntryFromFile(f.FullName, f.FullName.Substring(folder.Length));
                }
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, this);
        }
    }

    /// <summary> A singular function for error messages, can be re-routed from here, logged to a file, etc. </summary>
    /// <param name="message">The error message.</param>
    /// <param name="originator">The handle to the object making the error logging call.</param>
    private static void LogError(string message, Object? originator = null)
    {
        string callerName = "";
        if (originator != null)
        {
            var t = originator.GetType();
            if (t.FullName != null)
                callerName = "Error in: " + t.FullName + Environment.NewLine;
        }
        Console.WriteLine(callerName + message);
    }
}
