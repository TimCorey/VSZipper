using System.IO.Compression;

namespace ZipperLibrary;

public class Zipper
{
    private List<string> exclusions = new();

    public Zipper()
    {
        exclusions.Add(@"\bin\");
        exclusions.Add(@"\obj\");
        exclusions.Add(@"\.vs\");
        exclusions.Add(@"\.git\");
        exclusions.Add(@".exe");
        exclusions.Add(@".zip");
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
                if (exclusions.Where(x => f.FullName.Contains(x)).Count() == 0)
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
