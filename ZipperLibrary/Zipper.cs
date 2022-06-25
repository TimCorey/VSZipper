using System.IO.Compression;

using Microsoft.Extensions.Configuration;

namespace ZipperLibrary;

/// <summary>
/// Provides methods for creating a ZIP file for a Visual Studio solution or 
/// project folder.
/// </summary>
public class Zipper
{
    private List<string> exclusions = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Zipper"/> class.
    /// </summary>
    /// <param name="config">An instance of <see cref="IConfiguration"/> 
    /// used to load required data from a configuration data source.</param>
    public Zipper(IConfiguration config)
    {
        IConfigurationSection? exclusionSection = config.GetSection("Exclusions");
        string[]? excludedPathsAndExtensions = exclusionSection.Get<string[]>();

        foreach (string excludedPathOrExtension in excludedPathsAndExtensions)
        {
            exclusions.Add(excludedPathOrExtension);
        }
    }

    /// <summary>
    /// Gets the name to be used for naming the generated ZIP file.
    /// </summary>
    /// <returns>A <see cref="Task"/> object with the string representing 
    /// the file name.</returns>
    /// 
    /// <remarks>
    /// <para>
    /// This method generates the file name prefix in the following order
    /// of precedence:
    /// </para>
    /// <list type="number">
    ///     <item>
    ///         <term>Solution File</term>
    ///         <description>
    ///             Picks the name of the first file with ".sln" extension, 
    ///             excluding the file extension part.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term>Project File</term>
    ///         <description>
    ///             If no solution file is found, picks the name of the first file with ".csproj" extension, 
    ///             excluding the file extension part.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term>Default File Name Prefix</term>
    ///         <description>
    ///             If neither a solution file nor a project file exists in 
    ///             the current directory then the default prefix of 
    ///             "VSZipper is used.
    ///         </description>
    ///     </item>
    /// </list>
    /// <para>
    /// It then adds a suffix to the file name prefix using the current time 
    /// stamp, accurate to the second.
    /// </para>
    /// </remarks>
    private static async Task<string> GetFileName()
    {
        // Get the file name prefix:
        string folder = AppDomain.CurrentDomain.BaseDirectory;
        string filePrefix = string.Empty;
        FileInfo[] files;

        await Task.Run(() =>
        {
            files = new DirectoryInfo(folder).GetFiles("*.sln");

            if (!files.Any())
            {
                files = new DirectoryInfo(folder).GetFiles("*.csproj");
            }

            if (!files.Any())
            {
                filePrefix = "VSZipper";
            }
            else
            {
                filePrefix = files.First().Name[..^4];
            }
        });

        // Get the file name suffix:
        DateTime currentDate = DateTime.Now;
        currentDate = currentDate.AddTicks(
            -(currentDate.Ticks % TimeSpan.TicksPerSecond)); // Removes milliseconds.
        string fileSuffix = currentDate.ToString("yyyyMMddHHmmss");

        return $"{filePrefix}-{fileSuffix}.zip";
    }

    /// <summary>
    /// Creates a ZIP file containing folders, sub-folders and files in 
    /// the current directory, excluding items specified in the 
    /// exclusions list.
    /// </summary>
    /// <returns>An empty <see cref="Task"/> object.</returns>
    public async Task Zip()
    {
        string folder = AppDomain.CurrentDomain.BaseDirectory;
        string zipFile = await GetFileName();
        await Zip(folder, zipFile);
    }

    /// <summary>
    /// Creates a ZIP file containing folders, sub-folders and files in 
    /// the current directory, excluding items specified in the 
    /// exclusions list.
    /// </summary>
    /// <param name="folder">The folder whose contents are to be zipped up.</param>
    /// <param name="zipFile">The name of the generated ZIP file.</param>
    /// <returns>An empty <see cref="Task"/> object.</returns>
    public async Task Zip(string folder, string zipFile)
    {
        try
        {
            using ZipArchive archive = ZipFile.Open(zipFile, ZipArchiveMode.Create);

            var files = await Task.Run(() =>
            {
                var dirInfo = new DirectoryInfo(folder)
                 .GetFiles("*", SearchOption.AllDirectories)
                 .Where(x => (x.Attributes & FileAttributes.Hidden) == 0 && x.FullName != zipFile);
                return dirInfo;
            });

            foreach (var f in files)
            {
                if (!exclusions.Where(x => f.FullName.Contains(x)).Any())
                {
                    archive.CreateEntryFromFile(f.FullName, f.FullName.Substring(folder.Length));
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}